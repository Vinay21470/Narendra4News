"""Integration test with SQLite or a disposable SQL Server database and Azurite."""
import base64
import json
import os
from pathlib import Path
import secrets
import socket
import subprocess
import tempfile
import time
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
API = ROOT / 'backend' / 'Narendra4News.API'
BASE = 'http://127.0.0.1:5000/api'
PASSWORD = secrets.token_urlsafe(25) + 'Aa1!'
SQL_CONNECTION = os.environ.get('N4N_TEST_SQL_CONNECTION')

def request(method, path, payload=None, token=None):
    body = json.dumps(payload).encode() if payload is not None else None
    headers = {'Content-Type': 'application/json'}
    if token:
        headers['Authorization'] = 'Bearer ' + token
    try:
        with urllib.request.urlopen(urllib.request.Request(BASE + path, data=body, headers=headers, method=method)) as response:
            return json.load(response)
    except urllib.error.HTTPError as exc:
        raise AssertionError(f'{method} {path}: HTTP {exc.code}: {exc.read()[:300]!r}') from exc

with tempfile.TemporaryDirectory(prefix='n4n-flow-') as directory:
    env = {**os.environ, 'ASPNETCORE_ENVIRONMENT': 'Development', 'DatabaseProvider': 'Sqlite',
           'ConnectionStrings__Sqlite': f'Data Source={directory}/test.db',
           'AzureStorage__ConnectionString': 'UseDevelopmentStorage=true',
           'ADMIN_EMAIL': 'admin@example.test', 'ADMIN_PASSWORD': PASSWORD}
    if SQL_CONNECTION:
        # The connection must name a dedicated disposable test database.
        env['DatabaseProvider'] = 'SqlServer'
        env['ConnectionStrings__Sql'] = SQL_CONNECTION
    azlog = open(f'{directory}/azurite.log', 'w')
    apilog = open(f'{directory}/api.log', 'w')
    azurite = subprocess.Popen(['azurite', '--silent', '--location', directory], stdout=azlog, stderr=subprocess.STDOUT)
    api = subprocess.Popen(['dotnet', 'run', '--no-build', '--configuration', 'Release', '--urls', 'http://127.0.0.1:5000'], cwd=API, env=env, stdout=apilog, stderr=subprocess.STDOUT)
    try:
        for _ in range(450):
            try:
                with socket.create_connection(('127.0.0.1', 5000), 0.2):
                    break
            except OSError:
                time.sleep(0.2)
        else:
            raise AssertionError('API did not start: ' + Path(apilog.name).read_text()[-1200:])
        try:
            request('GET', '/admin/articles')
            raise AssertionError('Anonymous access to admin articles was allowed')
        except AssertionError as exc:
            assert 'HTTP 401' in str(exc), str(exc)
        token = request('POST', '/auth/login?useCookies=false', {'email': 'admin@example.test', 'password': PASSWORD})['accessToken']
        png = base64.b64decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScL/nwAAAABJRU5ErkJggg==')
        boundary = 'n4n-test'
        body = (f'--{boundary}\r\nContent-Disposition: form-data; name="file"; filename="test.png"\r\nContent-Type: image/png\r\n\r\n').encode() + png + (f'\r\n--{boundary}--\r\n').encode()
        upload = urllib.request.Request(BASE + '/media/upload', data=body, headers={'Content-Type': 'multipart/form-data; boundary=' + boundary, 'Authorization': 'Bearer ' + token}, method='POST')
        with urllib.request.urlopen(upload) as response:
            image = json.load(response)['data']['url']
        with urllib.request.urlopen('http://127.0.0.1:5000' + image) as response:
            assert response.read() == png
        categories = request('GET', '/categories')['data']
        category_id = next(x['id'] for x in categories if x['slug'] == 'box-office')
        movie = request('POST', '/movies', {'name': 'Sample Test Film', 'slug': 'sample-test-film', 'description': 'TEST ONLY', 'posterUrl': image, 'heroImageUrl': image, 'releaseDate': '2026-09-27', 'hero': None, 'director': None, 'producer': None, 'productionHouse': None, 'genre': None, 'language': 'Telugu', 'budget': None, 'runtime': None, 'certification': None}, token)['data']
        request('POST', f'/movies/{movie}/collections', {'collectionDate': '2026-09-27', 'dayNumber': 1, 'indiaNet': 12.5, 'indiaGross': 15, 'overseas': 3, 'worldwideGross': 18, 'openingDay': 18, 'weekendCollection': None, 'totalCollection': 18, 'notes': 'TEST ONLY'}, token)
        slug = 'sample-test-film-day-1-collections'
        request('POST', '/articles', {'title': 'Sample Test Film Day 1 Collections', 'slug': slug, 'shortDescription': 'TEST ONLY', 'content': 'Test article content.', 'featuredImageUrl': image, 'categoryId': category_id, 'movieId': movie, 'status': 'Published', 'isFeatured': True, 'isTrending': True, 'seoTitle': 'Sample Test', 'seoDescription': 'Test only', 'seoKeywords': 'test'}, token)
        assert any(x['slug'] == slug for x in request('GET', '/admin/articles', token=token)['data'])
        assert any(x['slug'] == slug for x in request('GET', '/articles')['data'])
        assert request('GET', '/articles/' + slug)['data']['views'] >= 1
        found = request('GET', '/search?q=Sample%20Test%20Film')['data']
        assert any(x['slug'] == slug for x in found['articles'])
        assert any(x['slug'] == 'sample-test-film' for x in found['movies'])
        history = request('GET', '/movies/sample-test-film')['data']
        assert len(history['movie']['collections']) == 1 and len(history['articles']) == 1
        print('PASS (' + ('SQL Server' if SQL_CONNECTION else 'SQLite') + '): login -> image -> movie -> collection -> publish -> view -> search -> history')
    finally:
        api.terminate()
        azurite.terminate()
        try: api.wait(timeout=5)
        except subprocess.TimeoutExpired: api.kill(); api.wait()
        azurite.kill(); azurite.wait()
        apilog.close(); azlog.close()
