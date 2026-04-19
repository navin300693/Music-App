import Link from "next/link";

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center bg-zinc-950 text-white">
      <h1 className="text-4xl font-bold mb-2">🎵 Music App</h1>
      <p className="text-zinc-400 mb-8">Stream music based on your plan</p>

      <div className="flex gap-4">
        <Link
          href="/login"
          className="px-6 py-3 bg-white text-black rounded-full font-medium hover:bg-zinc-200 transition"
        >
          Login
        </Link>
        <Link
          href="/register"
          className="px-6 py-3 border border-white rounded-full font-medium hover:bg-white hover:text-black transition"
        >
          Register
        </Link>
      </div>
    </main>
  );
}
