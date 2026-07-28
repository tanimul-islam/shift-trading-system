export default function HomePage() {
  return (
    <main className="flex min-h-screen items-center justify-center px-6">
      <section className="w-full max-w-xl text-center">
        <p className="mb-3 text-sm font-medium uppercase tracking-[0.2em] text-muted-foreground">
          Shift Trading System
        </p>

        <h1 className="text-4xl font-bold tracking-tight sm:text-5xl">
          Manage shift trades without manual tracking
        </h1>

        <p className="mt-5 text-base leading-7 text-muted-foreground sm:text-lg">
          Post shifts, accept available work, and keep track of hours owed in
          one place.
        </p>
      </section>
    </main>
  );
}
