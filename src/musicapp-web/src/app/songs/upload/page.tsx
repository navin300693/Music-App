"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { uploadSong, clearTokens } from "@/lib/api";

const ACCEPTED = ".mp3,.wav,.flac";

export default function UploadSongPage() {
  const router = useRouter();

  const [title, setTitle] = useState("");
  const [artist, setArtist] = useState("");
  const [album, setAlbum] = useState("");
  const [releasedDate, setReleasedDate] = useState("");
  const [file, setFile] = useState<File | null>(null);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  async function handleSubmit(e: React.SubmitEvent) {
    e.preventDefault();
    if (!file) { setError("Please select an audio file."); return; }

    setError("");
    setLoading(true);

    try {
      await uploadSong({ title, artist, album, releasedDate, file });
      setSuccess(true);
    } catch (err) {
      if (err instanceof Error && err.message.includes("Session expired")) {
        clearTokens();
        router.push("/login");
        return;
      }
      setError(err instanceof Error ? err.message : "Upload failed");
    } finally {
      setLoading(false);
    }
  }

  if (success) {
    return (
      <main className="flex min-h-screen items-center justify-center bg-zinc-950 text-white">
        <div className="text-center flex flex-col gap-4">
          <p className="text-4xl">✓</p>
          <p className="text-xl font-semibold">Song uploaded!</p>
          <div className="flex gap-3 justify-center mt-2">
            <button
              onClick={() => { setSuccess(false); setTitle(""); setArtist(""); setAlbum(""); setReleasedDate(""); setFile(null); }}
              className="px-4 py-2 rounded-full bg-zinc-800 hover:bg-zinc-700 transition text-sm"
            >
              Upload another
            </button>
            <Link href="/songs" className="px-4 py-2 rounded-full bg-white text-black hover:bg-zinc-200 transition text-sm">
              Back to songs
            </Link>
          </div>
        </div>
      </main>
    );
  }

  return (
    <main className="min-h-screen bg-zinc-950 text-white p-8">
      <div className="mb-6">
        <Link href="/songs" className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-zinc-800 hover:bg-zinc-700 text-sm text-white transition">
          ← Back to Songs
        </Link>
      </div>

      <div className="max-w-lg mx-auto">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold">Upload Song</h1>
          <Link href="/songs/upload/bulk" className="text-sm text-zinc-400 hover:text-white transition">
            Upload multiple →
          </Link>
        </div>

        <form onSubmit={handleSubmit} className="flex flex-col gap-5">

          <label className="flex flex-col gap-1">
            <span className="text-sm text-zinc-400">Title <span className="text-red-400">*</span></span>
            <input
              type="text"
              value={title}
              onChange={e => setTitle(e.target.value)}
              placeholder="Song title"
              required
              className="px-4 py-2 rounded-lg bg-zinc-800 border border-zinc-700 text-white focus:outline-none focus:border-white transition"
            />
          </label>

          <label className="flex flex-col gap-1">
            <span className="text-sm text-zinc-400">Artist <span className="text-red-400">*</span></span>
            <input
              type="text"
              value={artist}
              onChange={e => setArtist(e.target.value)}
              placeholder="Artist name"
              required
              className="px-4 py-2 rounded-lg bg-zinc-800 border border-zinc-700 text-white focus:outline-none focus:border-white transition"
            />
          </label>

          <label className="flex flex-col gap-1">
            <span className="text-sm text-zinc-400">Album</span>
            <input
              type="text"
              value={album}
              onChange={e => setAlbum(e.target.value)}
              placeholder="Album name (optional)"
              className="px-4 py-2 rounded-lg bg-zinc-800 border border-zinc-700 text-white focus:outline-none focus:border-white transition"
            />
          </label>

          <label className="flex flex-col gap-1">
            <span className="text-sm text-zinc-400">Release date <span className="text-red-400">*</span></span>
            <input
              type="date"
              value={releasedDate}
              onChange={e => setReleasedDate(e.target.value)}
              required
              className="px-4 py-2 rounded-lg bg-zinc-800 border border-zinc-700 text-white focus:outline-none focus:border-white transition"
            />
          </label>

          <label className="flex flex-col gap-1 cursor-pointer">
            <span className="text-sm text-zinc-400">Audio file <span className="text-red-400">*</span></span>
            <div className={`px-4 py-4 rounded-lg border border-dashed transition flex flex-col items-center gap-2
              ${file ? "border-zinc-500 bg-zinc-800/50" : "border-zinc-700 bg-zinc-900 hover:border-zinc-500"}`}>
              <span className="text-2xl">{file ? "🎵" : "📂"}</span>
              <span className="text-sm text-zinc-400 text-center">
                {file ? file.name : "Click to choose an MP3, WAV or FLAC"}
              </span>
              {file && (
                <span className="text-xs text-zinc-500">{(file.size / 1024 / 1024).toFixed(1)} MB</span>
              )}
              <input
                type="file"
                accept={ACCEPTED}
                onChange={e => setFile(e.target.files?.[0] ?? null)}
                className="hidden"
                required
              />
            </div>
          </label>

          {error && <p className="text-red-400 text-sm">{error}</p>}

          <button
            type="submit"
            disabled={loading}
            className="py-2.5 rounded-full bg-white text-black font-medium hover:bg-zinc-200 transition disabled:opacity-50 mt-2"
          >
            {loading ? "Uploading…" : "Upload"}
          </button>

        </form>
      </div>
    </main>
  );
}