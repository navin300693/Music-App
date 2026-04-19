const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

if (!BASE_URL) {
  throw new Error("NEXT_PUBLIC_API_URL is not set. Add it to your .env.local file.");
}

// ---------- token storage ----------

export function saveTokens(accessToken: string, refreshToken: string) {
  localStorage.setItem("token", accessToken);
  localStorage.setItem("refreshToken", refreshToken);
}

export function clearTokens() {
  localStorage.removeItem("token");
  localStorage.removeItem("refreshToken");
}

// ---------- auth ----------

export async function registerUser(email: string, password: string): Promise<void> {
  const response = await fetch(`${BASE_URL}/api/v1/identity/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail ?? "Registration failed");
  }
}

export async function loginUser(email: string, password: string): Promise<void> {
  const response = await fetch(`${BASE_URL}/api/v1/identity/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail ?? "Login failed");
  }

  const data = await response.json();
  saveTokens(data.accessToken, data.refreshToken);
}

async function refreshAccessToken(): Promise<string> {
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) throw new Error("No refresh token");

  const response = await fetch(`${BASE_URL}/api/v1/identity/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
  });

  if (!response.ok) {
    clearTokens();
    throw new Error("Session expired. Please log in again.");
  }

  const data = await response.json();
  saveTokens(data.accessToken, data.refreshToken);
  return data.accessToken as string;
}

// Fetches with the stored access token, refreshes once on 401, then retries.
// Throws "Session expired" if refresh also fails — callers should redirect to /login.
async function authFetch(input: RequestInfo, init?: RequestInit): Promise<Response> {
  const token = localStorage.getItem("token");

  const response = await fetch(input, {
    ...init,
    headers: { ...init?.headers, Authorization: `Bearer ${token}` },
  });

  if (response.status !== 401) return response;

  const newToken = await refreshAccessToken();

  return fetch(input, {
    ...init,
    headers: { ...init?.headers, Authorization: `Bearer ${newToken}` },
  });
}

// ---------- upload ----------

export async function uploadSong(fields: {
  title: string;
  artist: string;
  album: string;
  releasedDate: string;
  file: File;
}): Promise<string> {
  const form = new FormData();
  form.append("title", fields.title);
  form.append("artist", fields.artist);
  if (fields.album) form.append("album", fields.album);
  form.append("releasedDate", fields.releasedDate);
  form.append("songFile", fields.file);

  const response = await authFetch(`${BASE_URL}/api/v1/songs`, {
    method: "POST",
    body: form,
  });

  if (!response.ok) {
    const err = await response.json().catch(() => ({}));
    throw new Error(err.title ?? "Upload failed");
  }

  const data = await response.json();
  return data.id as string;
}

export interface BulkSongEntry {
  title: string;
  artist: string;
  album: string;
  releasedDate: string;
  file: File;
}

export async function uploadBulkSongs(entries: BulkSongEntry[]): Promise<string[]> {
  const form = new FormData();
  entries.forEach((e, i) => {
    form.append(`[${i}].title`, e.title);
    form.append(`[${i}].artist`, e.artist);
    if (e.album) form.append(`[${i}].album`, e.album);
    form.append(`[${i}].releasedDate`, e.releasedDate);
    form.append(`[${i}].songFile`, e.file);
  });

  const response = await authFetch(`${BASE_URL}/api/v1/songs/bulk`, {
    method: "POST",
    body: form,
  });

  if (!response.ok) {
    const err = await response.json().catch(() => ({}));
    throw new Error(err.title ?? "Bulk upload failed");
  }

  const data = await response.json();
  return data.createdIds as string[];
}

// ---------- songs ----------

export interface Song {
  id: string;
  title: string;
  artist: string;
  album: string | null;
  releasedDate: string;
  duration: string;
}

export async function getSongs(): Promise<Song[]> {
  const response = await authFetch(`${BASE_URL}/api/v1/songs`);

  if (!response.ok) throw new Error("Failed to fetch songs");

  return response.json() as Promise<Song[]>;
}

// ---------- skip ----------

// Returns true if skip is allowed, false if the plan forbids it.
export async function requestSkip(): Promise<boolean> {
  const response = await authFetch(`${BASE_URL}/api/v1/playback/skip`, { method: "POST" });
  if (response.status === 403) return false;
  if (!response.ok) throw new Error("Skip request failed");
  return true;
}

// ---------- playback ----------

export interface PlaybackStream {
  songId: string;
  streamUrl: string;
  audioQuality: string;
  supportsOffline: boolean;
}

export async function getStreamUrl(songId: string): Promise<PlaybackStream> {
  const response = await authFetch(`${BASE_URL}/api/v1/playback/stream/${songId}`);

  if (!response.ok) throw new Error("Failed to get stream URL");

  return response.json() as Promise<PlaybackStream>;
}