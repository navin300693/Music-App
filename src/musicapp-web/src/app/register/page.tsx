"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import AuthCard from "@/components/AuthCard";
import { registerUser } from "@/lib/api";

type PlanType = "Free" | "Premium" | "Artist";

export default function RegisterPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [planType, setPlanType] = useState<PlanType>("Free");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  return (
    <AuthCard title="Register">
      <form
        onSubmit={async (e) => {
          e.preventDefault();
          setError("");
          setLoading(true);

          try {
            await registerUser(email, password);
            router.push("/login");
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

        <div>
          <label className="block text-sm text-zinc-400 mb-1">Plan</label>
          <select
            value={planType}
            onChange={(e) => setPlanType(e.target.value as PlanType)}
            className="w-full px-4 py-2 rounded-lg bg-zinc-800 border border-zinc-700 text-white focus:outline-none focus:border-white"
          >
            <option value="Free">Free</option>
            <option value="Premium">Premium</option>
            <option value="Artist">Artist</option>
          </select>
        </div>

        {error && (
          <p className="text-red-400 text-sm">{error}</p>
        )}

        <button
          type="submit"
          disabled={loading}
          className="w-full py-2 bg-white text-black rounded-full font-medium hover:bg-zinc-200 transition mt-2 disabled:opacity-50"
        >
          {loading ? "Registering..." : "Register"}
        </button>
      </form>

      <p className="text-center text-zinc-400 text-sm mt-6">
        Already have an account?{" "}
        <Link href="/login" className="text-white underline">
          Login
        </Link>
      </p>
    </AuthCard>
  );
}