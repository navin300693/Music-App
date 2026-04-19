"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import {
  getSongs,
  getStreamUrl,
  requestSkip,
  Song,
  clearTokens,
} from "@/lib/api";

function gradientFromString(str: string) {
  let hash = 0;
  for (let i = 0; i < str.length; i++)
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  const h1 = Math.abs(hash % 360);
  const h2 = (h1 + 50) % 360;
  return `linear-gradient(135deg, hsl(${h1},60%,35%), hsl(${h2},60%,20%))`;
}

function formatTime(secs: number) {
  if (!isFinite(secs) || isNaN(secs)) return "0:00";
  const m = Math.floor(secs / 60);
  const s = Math.floor(secs % 60);
  return `${m}:${s.toString().padStart(2, "0")}`;
}

export default function SongsPage() {
  const router = useRouter();
  const audioRef = useRef<HTMLAudioElement>(null);

  const [songs, setSongs] = useState<Song[]>([]);
  const [pageError, setPageError] = useState("");
  const [loading, setLoading] = useState(true);

  const [currentIndex, setCurrentIndex] = useState<number | null>(null);
  const [playing, setPlaying] = useState(false);
  const [loadingStream, setLoadingStream] = useState(false);
  const [streamError, setStreamError] = useState("");
  const [skipError, setSkipError] = useState("");

  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [volume, setVolume] = useState(1);
  const [quality, setQuality] = useState("");

  const currentSong =
    currentIndex !== null ? (songs[currentIndex] ?? null) : null;

  useEffect(() => {
    if (!localStorage.getItem("token")) {
      router.push("/login");
      return;
    }
    getSongs()
      .then(setSongs)
      .catch((err) => {
        if (err instanceof Error && err.message.includes("Session expired")) {
          clearTokens();
          router.push("/login");
        } else {
          setPageError(
            err instanceof Error ? err.message : "Failed to load songs",
          );
        }
      })
      .finally(() => setLoading(false));
  }, [router]);

  async function loadAndPlay(index: number) {
    setStreamError("");
    setSkipError("");
    setCurrentIndex(index);
    setPlaying(false);
    setLoadingStream(true);
    setCurrentTime(0);
    setDuration(0);

    try {
      const stream = await getStreamUrl(songs[index].id);
      const audio = audioRef.current;
      if (!audio) return;
      setQuality(stream.audioQuality);
      audio.src = stream.streamUrl;
      audio.volume = volume;
      audio.load();
      await audio.play();
      setPlaying(true);
    } catch (err) {
      if (err instanceof Error && err.message.includes("Session expired")) {
        clearTokens();
        router.push("/login");
        return;
      }
      setStreamError("Could not play this song.");
    } finally {
      setLoadingStream(false);
    }
  }

  function togglePlay() {
    const audio = audioRef.current;
    if (!audio) return;
    if (playing) {
      audio.pause();
      setPlaying(false);
    } else {
      audio.play();
      setPlaying(true);
    }
  }

  function playNext() {
    if (currentIndex === null || songs.length === 0) return;
    loadAndPlay((currentIndex + 1) % songs.length);
  }

  function playRandom() {
    if (songs.length <= 1) return;
    let randomIndex: number;
    do {
      randomIndex = Math.floor(Math.random() * songs.length);
    } while (randomIndex === currentIndex);
    loadAndPlay(randomIndex);
  }

  function playPrev() {
    if (currentIndex === null || songs.length === 0) return;
    const audio = audioRef.current;
    if (audio && audio.currentTime > 3) {
      audio.currentTime = 0;
    } else {
      loadAndPlay((currentIndex - 1 + songs.length) % songs.length);
    }
  }

  async function seek(value: number) {
    const audio = audioRef.current;
    if (!audio) return;

    if (value > currentTime) {
      setSkipError("");
      const allowed = await requestSkip();
      if (!allowed) {
        setSkipError("Your plan doesn't allow seeking forward.");
        return;
      }
    }

    audio.currentTime = value;
    setCurrentTime(value);
  }

  function changeVolume(value: number) {
    const audio = audioRef.current;
    if (audio) audio.volume = value;
    setVolume(value);
  }

  if (loading)
    return (
      <main className="flex min-h-screen items-center justify-center bg-zinc-950 text-white">
        <p className="text-zinc-400">Loading songs...</p>
      </main>
    );

  if (pageError)
    return (
      <main className="flex min-h-screen items-center justify-center bg-zinc-950 text-white">
        <p className="text-red-400">{pageError}</p>
      </main>
    );

  return (
    <main className="min-h-screen bg-zinc-950 text-white p-8 pb-36">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold">Songs</h1>
        <Link
          href="/songs/upload"
          className="px-4 py-2 rounded-full bg-white text-black text-sm font-medium hover:bg-zinc-200 transition mr-10"
        >
          + Upload
        </Link>
      </div>

      {songs.length === 0 ? (
        <p className="text-zinc-400">No songs found.</p>
      ) : (
        <div className="flex flex-col gap-2">
          {songs.map((song, i) => {
            const isActive = currentIndex === i;
            const label = (song.album ?? song.title).slice(0, 2).toUpperCase();
            return (
              <div
                key={song.id}
                onClick={() => (isActive ? togglePlay() : loadAndPlay(i))}
                className={`flex items-center gap-4 rounded-xl px-4 py-3 cursor-pointer transition select-none
                  ${isActive ? "bg-zinc-700 ring-1 ring-zinc-500" : "bg-zinc-900 hover:bg-zinc-800"}`}
              >
                <div
                  className="w-12 h-12 rounded-lg flex-shrink-0 flex items-center justify-center text-sm font-bold text-white/60"
                  style={{
                    background: gradientFromString(song.album ?? song.title),
                  }}
                >
                  {label}
                </div>

                <div className="flex-1 min-w-0">
                  <p className="font-medium truncate">{song.title}</p>
                  <p className="text-sm text-zinc-400 truncate">
                    {song.artist}
                    {song.album ? ` · ${song.album}` : ""}
                  </p>
                </div>

                <div className="flex items-center gap-3 flex-shrink-0">
                  <span className="text-sm text-zinc-500 tabular-nums">
                    {song.duration}
                  </span>
                  <span className="w-5 text-center text-sm">
                    {isActive && loadingStream ? (
                      <span className="animate-spin inline-block">↻</span>
                    ) : isActive && playing ? (
                      "⏸"
                    ) : (
                      "▶"
                    )}
                  </span>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {currentSong && (
        <div className="fixed bottom-0 left-0 right-0 bg-zinc-900/95 backdrop-blur-md border-t border-zinc-800 px-6 pt-2 pb-4">
          {/* Seek bar */}
          <div className="flex items-center gap-3 mb-3">
            <span className="text-xs text-zinc-500 w-10 text-right tabular-nums">
              {formatTime(currentTime)}
            </span>
            <input
              type="range"
              min={0}
              max={duration || 0}
              step={0.5}
              value={currentTime}
              onChange={(e) => seek(Number(e.target.value))}
              className="flex-1 h-1 accent-white cursor-pointer"
            />
            <span className="text-xs text-zinc-500 w-10 tabular-nums">
              {formatTime(duration)}
            </span>
          </div>

          {/* Controls */}
          <div className="flex items-center gap-4">
            {/* Song info */}
            <div className="flex items-center gap-3 w-56 min-w-0">
              <div
                className="w-10 h-10 rounded-md flex-shrink-0 flex items-center justify-center text-xs font-bold text-white/60"
                style={{
                  background: gradientFromString(
                    currentSong.album ?? currentSong.title,
                  ),
                }}
              >
                {(currentSong.album ?? currentSong.title)
                  .slice(0, 2)
                  .toUpperCase()}
              </div>
              <div className="min-w-0">
                <p className="text-sm font-medium truncate">
                  {currentSong.title}
                </p>
                <p className="text-xs text-zinc-400 truncate">
                  {currentSong.artist}
                </p>
              </div>
            </div>

            {/* Playback */}
            <div className="flex items-center gap-5 flex-1 justify-center">
              <button
                onClick={playPrev}
                className="text-zinc-400 hover:text-white transition text-xl leading-none"
              >
                ⏮
              </button>
              <button
                onClick={togglePlay}
                disabled={loadingStream}
                className="w-11 h-11 rounded-full bg-white text-black flex items-center justify-center text-base hover:bg-zinc-200 transition disabled:opacity-50"
              >
                {loadingStream ? "…" : playing ? "⏸" : "▶"}
              </button>
              <button
                onClick={playNext}
                className="text-zinc-400 hover:text-white transition text-xl leading-none"
              >
                ⏭
              </button>
              <button
                onClick={playRandom}
                title="Shuffle"
                className="text-zinc-400 hover:text-white transition text-xl leading-none"
              >
                🔀
              </button>
            </div>

            {/* Volume + meta */}
            <div className="flex items-center gap-3 w-56 justify-end">
              {(streamError || skipError) && (
                <span className="text-xs text-red-400 truncate">
                  {streamError || skipError}
                </span>
              )}
              {quality && !streamError && !skipError && (
                <span className="text-xs px-2 py-0.5 rounded-full bg-zinc-700 text-zinc-300 flex-shrink-0">
                  {quality}
                </span>
              )}
              <span className="text-zinc-500 text-sm flex-shrink-0">🔊</span>
              <input
                type="range"
                min={0}
                max={1}
                step={0.01}
                value={volume}
                onChange={(e) => changeVolume(Number(e.target.value))}
                className="w-20 h-1 accent-white cursor-pointer"
              />
            </div>
          </div>
        </div>
      )}

      <audio
        ref={audioRef}
        onTimeUpdate={() => setCurrentTime(audioRef.current?.currentTime ?? 0)}
        onLoadedMetadata={() => setDuration(audioRef.current?.duration ?? 0)}
        onEnded={playNext}
      />
    </main>
  );
}
