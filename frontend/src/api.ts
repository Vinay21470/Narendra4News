import axios from 'axios';
export const api=axios.create({baseURL:import.meta.env.VITE_API_BASE_URL || (import.meta.env.DEV ? 'http://localhost:5000/api' : '/api')});
export type Article={id:number;title:string;slug:string;shortDescription?:string;content?:string;featuredImageUrl?:string;publishedDate?:string;updatedDate?:string;views:number;likes:number;category:string;categorySlug:string;isFeatured:boolean;isTrending:boolean;seoTitle?:string;seoDescription?:string;movieId?:number;categoryId?:number;status?:string};
export type Movie={id:number;name:string;slug:string;posterUrl?:string;heroImageUrl?:string;description?:string;releaseDate?:string;director?:string;collections?:Collection[]};
export type Collection={id:number;dayNumber:number;collectionDate:string;indiaNet?:number;indiaGross?:number;overseas?:number;worldwideGross?:number;notes?:string};
export type Category={id:number;name:string;slug:string;active:boolean};
export const data=<T,>(path:string)=>api.get<{success:boolean;data:T}>(path).then(r=>r.data.data);
export const page=<T,>(path:string)=>api.get<{data:T[];total:number}>(path).then(r=>r.data);
export const errorMessage=(error:unknown)=>axios.isAxiosError(error)?(error.response?.data?.message || error.message):String(error);
