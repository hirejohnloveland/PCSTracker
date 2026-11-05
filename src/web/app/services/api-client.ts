import Service from '@ember/service';
import type { components } from 'web/types/api';

type ProblemDetails = components['schemas']['ApiProblemDetails'];
export type ProblemError = components['schemas']['ProblemError'];

export type QueryValue = string | number | undefined;

interface ApiErrorInit {
  status: number;
  title: string;
  detail?: string | null;
  type?: string | null;
  errors?: ProblemError[];
  traceId?: string | null;
}

export class ApiError extends Error {
  readonly status: number;
  readonly title: string;
  readonly detail: string | null;
  readonly type: string | null;
  readonly errors: ProblemError[];
  readonly traceId: string | null;

  constructor(init: ApiErrorInit) {
    super(init.detail ?? init.title);
    this.name = 'ApiError';
    this.status = init.status;
    this.title = init.title;
    this.detail = init.detail ?? null;
    this.type = init.type ?? null;
    this.errors = init.errors ?? [];
    this.traceId = init.traceId ?? null;
  }
}

const API_BASE = '/api';

function toQueryString(query: Record<string, QueryValue>): string {
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== '') {
      params.set(key, String(value));
    }
  }
  const text = params.toString();
  return text === '' ? '' : `?${text}`;
}

async function toApiError(response: Response): Promise<ApiError> {
  let problem: Partial<ProblemDetails> | null = null;
  try {
    problem = (await response.json()) as Partial<ProblemDetails>;
  } catch {
    problem = null;
  }

  return new ApiError({
    status: response.status,
    title: problem?.title ?? response.statusText ?? 'The request failed.',
    detail: problem?.detail,
    type: problem?.type,
    errors: problem?.errors,
    traceId: problem?.traceId,
  });
}

export default class ApiClientService extends Service {
  async getJson<T>(
    path: string,
    query: Record<string, QueryValue> = {},
  ): Promise<T> {
    return this.send<T>('GET', `${path}${toQueryString(query)}`);
  }

  async postJson<T>(path: string, body?: unknown): Promise<T> {
    return this.send<T>('POST', path, body);
  }

  private async send<T>(
    method: string,
    url: string,
    body?: unknown,
  ): Promise<T> {
    const headers: Record<string, string> = {
      Accept: 'application/json, application/problem+json',
    };
    if (body !== undefined) {
      headers['Content-Type'] = 'application/json';
    }

    let response: Response;
    try {
      response = await fetch(`${API_BASE}${url}`, {
        method,
        headers,
        body: body === undefined ? undefined : JSON.stringify(body),
      });
    } catch {
      throw new ApiError({
        status: 0,
        title: 'The server could not be reached.',
      });
    }

    if (!response.ok) {
      throw await toApiError(response);
    }

    return (await response.json()) as T;
  }
}
