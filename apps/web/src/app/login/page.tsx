"use client";
import { SubmitEvent, useState } from "react";
import { Eye, EyeOff, LockKeyhole, Mail } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    setError("");
    setIsLoading(true);

    try {
      const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/auth/login`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            emailAddress: email,
            password,
          }),
        },
      );
      console.log(response.body);
      const data = await response.json();

      if (!response.ok) {
        setError(data.message ?? "Invalid email or password.");
        return;
      }

      console.log("Login successful:", data);
    } catch {
      setError("Unable to connect to the server.");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="relative green-gradient min-h-screen overflow-hidden">
      <div className="relative z-10 mx-auto flex min-h-screen w-full max-w-sm flex-col items-center justify-center px-5 text-center">
        <section className="w-full">
          <div className="mb-8">
            <h1 className="text-3xl font-semibold tracking-tight text-white">
              Welcome Back!
            </h1>
            <p className="mt-2 text-sm text-white/60">
              Log in to continue using Shift Trade
            </p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4 text-left">
            <div>
              <label htmlFor="email" className="text-sm text-white/80">
                Email
              </label>
              <div className="relative mt-2">
                <Mail
                  className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-white/50"
                  aria-hidden="true"
                />
                <input
                  id="email"
                  name="email"
                  type="email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  autoComplete="email"
                  required
                  placeholder="Enter your email"
                  className="h-12 w-full rounded-lg border border-white/20 bg-white/10 pl-10 pr-3 text-sm text-white outline-none placeholder:text-white/40 focus:border-accent focus:ring-2 focus:ring-accent/20"
                />
              </div>
            </div>

            <div>
              <label htmlFor="password" className="text-sm text-white/80">
                Password
              </label>
              <div className="relative mt-2">
                <LockKeyhole
                  className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-white/50"
                  aria-hidden="true"
                />
                <input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  name="password"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  autoComplete="current-password"
                  placeholder="Enter your password"
                  required
                  className="h-12 w-full rounded-lg border border-white/20 bg-white/10 pl-10 pr-10 text-sm text-white outline-none placeholder:text-white/40 focus:border-accent focus:ring-2 focus:ring-accent/20"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword((current) => !current)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-white/50 hover:text-white"
                  aria-label={showPassword ? "Hide Password" : "Show Password"}
                >
                  {showPassword ? (
                    <EyeOff className="size-4" />
                  ) : (
                    <Eye className="size-4" />
                  )}
                </button>
              </div>
            </div>

            {error ? (
              <p className="rounded-lg border border-red-200 bg-red-950/20 px-3 py-2 text-sm text-red-200">
                {error}
              </p>
            ) : null}

            <div className="flex items-center justify-between text-xs">
              <label className="flex items-center gap-2 text-accent">
                <input
                  type="checkbox"
                  className="size-4 rounded border border-border bg-transparent accent-primary"
                />
                Remember Me
              </label>

              <button type="button" className="text-muted hover:text-accent">
                Forget Password?
              </button>
            </div>
            <div className="flex justify-center">
              <Button
                type="submit"
                disabled={isLoading}
                className="w-1/3 text-accent text-md h-10 rounded-xl hover:text-secondary hover:bg-accent-foreground cursor-pointer disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isLoading ? "Signing in..." : "Sign in"}
              </Button>
            </div>
          </form>
        </section>
      </div>
    </main>
  );
}
