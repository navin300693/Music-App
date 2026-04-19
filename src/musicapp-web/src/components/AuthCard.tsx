interface AuthCardProps {
  title: string;
  children: React.ReactNode;
}

export default function AuthCard({ title, children }: AuthCardProps) {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center bg-zinc-950 text-white">
      <div className="w-full max-w-sm bg-zinc-900 rounded-2xl p-8">
        <h1 className="text-2xl font-bold mb-6 text-center">{title}</h1>
        {children}
      </div>
    </main>
  );
}