import axios from 'axios'

export class ApiError extends Error {
  readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5143',
})

api.interceptors.request.use((config) => {
  config.headers['Content-Type'] = 'application/json'
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status: number = error.response?.status ?? 0
    const message: string = error.response?.data?.error ?? error.message ?? 'Unknown error'
    return Promise.reject(new ApiError(message, status))
  }
)

export default api
