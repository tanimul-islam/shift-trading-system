import type { Metadata } from "next";
import localFont from "next/font/local";
import "./globals.css";
import { Geist } from "next/font/google";
import { cn } from "@/lib/utils";

const geist = Geist({subsets:['latin'],variable:'--font-sans'});

const violetSans = localFont({
  src: "../../public/font/VioletSans-Regular.woff2",
  variable: "--font-violet-sans",
  display: "swap",
  weight: "400",
  style: "normal",
});

export const metadata: Metadata = {
  title: {
    default: "Shift Trading System",
    template: "%s | Shift Trading System",
  },
  description:
    "Post shifts, accept available work, and track hours owed between employees.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className={cn("font-sans", geist.variable)}>
      <body className={`${violetSans.variable} antialiased`}>{children}</body>
    </html>
  );
}
