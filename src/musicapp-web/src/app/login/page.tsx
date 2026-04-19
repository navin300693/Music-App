"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import AuthCard from "@/components/AuthCard";
import { loginUser } from "@/lib/api";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  return (
    <AuthCard title="Login">
      <form
        onSubmit={async (e) => {
          e.preventDefault();
          setError("");
          setLoading(true);

          try {
            await loginUser(email, password);
            router.push("/songs");
          } catch (err) {
            setError(err instanceof Error ? err.message : "Something went wrong");
          } finally {
            setLoading(false);
          }
        }}
        className="flex flex-col gap-4"
      >
        <div>
          <label className="block text-sm text-zinc-400 mb-1">Email</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="w-full px-4 py-2 rounded-lg bg-zinc-800 border border-zinc-700 text-white focus:outline-none focus:border-white"
            placeholder="you@example.com"
            required
          />
        </div>

        <div>
          <label className="block text-sm text-zinc-400 mb-1">Password</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="w-full px-4 py-2 rounded-lg bg-zinc-800 border border-zinc-700 text-white focus:outline-none focus:border-white"
            placeholder="••••••••"
            required
          />
        </div>

        {error && (
          <p className="text-red-400 text-sm">{error}</p>
        )}

        <button
          type="submit"
          disabled={loading}
          className="w-full py-2 bg-white text-black rounded-full font-medium hover:bg-zinc-200 transition mt-2 disabled:opacity-50"
        >
          {loading ? "Logging in..." : "Login"}
        </button>
      </form>

      <p className="text-center text-zinc-400 text-sm mt-6">
        Don&apos;t have an account?{" "}
        <Link href="/register" className="text-white underline">
          Register
        </Link>
      </p>
    </AuthCard>
  );
}