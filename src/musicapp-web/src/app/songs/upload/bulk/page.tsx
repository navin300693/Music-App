"use client";

import { useState, useRef } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { uploadBulkSongs, BulkSongEntry, clearTokens } from "@/lib/api";

const ACCEPTED = ".mp3,.wav,.flac";
const TODAY = new Date().toISOString().split("T")[0];

interface Row extends BulkSongEntry {
  id: string;
}

function nameFromFile(file: File) {
  return file.name.replace(/\.[^.]+$/, "").replace(/[-_]/g, " ");
}

export default function BulkUploadPage() {
  const router = useRouter();
  const inputRef = useRef<HTMLInputElement>(null);

  const [rows, setRows] = useState<Row[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [successCount, setSuccessCount] = useState<number | null>(null);

  function addFiles(files: FileList | null) {
    if (!files) return;
    const newRows: Row[] = Array.from(files).map((file) => ({
      id: crypto.randomUUID(),
      file,
      title: nameFromFile(file),
      artist: "",
      album: "",
      releasedDate: TODAY,
    }));
    setRows((prev) => [...prev, ...newRows]);
  }

  function updateRow(id: string, field: keyof BulkSongEntry, value: string) {
    setRows((prev) =>
      prev.map((r) => (r.id === id ? { ...r, [field]: value } : r))
    );
  }

  function removeRow(id: string) {
    setRows((prev) => prev.filter((r) => r.id !== id));
  }

  async function handleSubmit(e: React.SubmitEvent) {
    e.preventDefault();
    if (rows.length === 0) { setError("Add at least one file."); return; }
    const missing = rows.find((r) => !r.title.trim() || !r.artist.trim() || !r.releasedDate);
    if (missing) { setError("Fill in title, artist and date for every song."); return; }

    setError("");
    setLoading(true);

    try {
      const ids = await uploadBulkSongs(rows);
      setSuccessCount(ids.length);
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

  if (successCount !== null) {
    return (
      <main className="flex min-h-screen items-center justify-center bg-zinc-950 text-white">
        <div className="text-center flex flex-col gap-4">
          <p className="text-4xl">✓</p>
          <p className="text-xl font-semibold">{successCount} song{successCount !== 1 ? "s" : ""} uploaded!</p>
          <div className="flex gap-3 justify-center mt-2">
            <button
              onClick={() => { setSuccessCount(null); setRows([]); }}
              className="px-4 py-2 rounded-full bg-zinc-800 hover:bg-zinc-700 transition text-sm"
            >
              Upload more
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
        <Link href="/songs/upload" className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-zinc-800 hover:bg-zinc-700 text-sm text-white transition">
          ← Back to Upload
        </Link>
      </div>

      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold mb-2">Bulk Upload</h1>
        <p className="text-zinc-400 text-sm mb-8">Add multiple songs at once. Fill in the details for each file before uploading.</p>

        {/* Drop zone */}
        <div
          onClick={() => inputRef.current?.click()}
          onDragOver={(e) => e.preventDefault()}
          onDrop={(e) => { e.preventDefault(); addFiles(e.dataTransfer.files); }}
          className="border-2 border-dashed border-zinc-700 hover:border-zinc-500 rounded-xl p-8 text-center cursor-pointer transition mb-6"
        >
          <p className="text-3xl mb-2">📂</p>
          <p className="text-zinc-300 font-medium">Drop files here or click to browse</p>
          <p className="text-zinc-500 text-sm mt-1">MP3, WAV, FLAC</p>
          <input
            ref={inputRef}
            type="file"
            accept={ACCEPTED}
            multiple
            className="hidden"
            onChange={(e) => addFiles(e.target.files)}
          />
        </div>

        {rows.length > 0 && (
          <form onSubmit={handleSubmit}>
            {/* Header */}
            <div className="grid grid-cols-[2fr_2fr_1.5fr_1.5fr_auto] gap-3 px-3 mb-2 text-xs text-zinc-500 uppercase tracking-wide">
              <span>Title</span>
              <span>Artist</span>
              <span>Album</span>
              <span>Release date</span>
              <span />
            </div>

            <div className="flex flex-col gap-2 mb-6">
              {rows.map((row) => (
                <div key={row.id} className="grid grid-cols-[2fr_2fr_1.5fr_1.5fr_auto] gap-3 items-center bg-zinc-900 rounded-xl px-3 py-3">
                  <div className="min-w-0">
                    <p className="text-xs text-zinc-500 truncate mb-1">{row.file.name}</p>
                    <input
                      type="text"
                      value={row.title}
                      onChange={(e) => updateRow(row.id, "title", e.target.value)}
                      placeholder="Title"
                      required
                      className="w-full px-3 py-1.5 rounded-lg bg-zinc-800 border border-zinc-700 text-sm text-white focus:outline-none focus:border-white transition"
                    />
                  </div>
                  <input
                    type="text"
                    value={row.artist}
                    onChange={(e) => updateRow(row.id, "artist", e.target.value)}
                    placeholder="Artist"
                    required
                    className="w-full px-3 py-1.5 rounded-lg bg-zinc-800 border border-zinc-700 text-sm text-white focus:outline-none focus:border-white transition"
                  />
                  <input
                    type="text"
                    value={row.album}
                    onChange={(e) => updateRow(row.id, "album", e.target.value)}
                    placeholder="Album"
                    className="w-full px-3 py-1.5 rounded-lg bg-zinc-800 border border-zinc-700 text-sm text-white focus:outline-none focus:border-white transition"
                  />
                  <input
                    type="date"
                    value={row.releasedDate}
                    onChange={(e) => updateRow(row.id, "releasedDate", e.target.value)}
                    required
                    className="w-full px-3 py-1.5 rounded-lg bg-zinc-800 border border-zinc-700 text-sm text-white focus:outline-none focus:border-white transition"
                  />
                  <button
                    type="button"
                    onClick={() => removeRow(row.id)}
                    className="text-zinc-500 hover:text-red-400 transition text-lg leading-none px-1"
                  >
                    ✕
                  </button>
                </div>
              ))}
            </div>

            {error && <p className="text-red-400 text-sm mb-4">{error}</p>}

            <div className="flex items-center gap-4">
              <button
                type="submit"
                disabled={loading}
                className="px-6 py-2.5 rounded-full bg-white text-black font-medium hover:bg-zinc-200 transition disabled:opacity-50"
              >
                {loading ? `Uploading ${rows.length} song${rows.length !== 1 ? "s" : ""}…` : `Upload ${rows.length} song${rows.length !== 1 ? "s" : ""}`}
              </button>
              <button
                type="button"
                onClick={() => setRows([])}
                className="px-4 py-2.5 rounded-full bg-zinc-800 hover:bg-zinc-700 text-sm transition"
              >
                Clear all
              </button>
            </div>
          </form>
        )}
      </div>
    </main>
  );
}