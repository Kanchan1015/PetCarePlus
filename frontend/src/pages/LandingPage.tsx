import { useEffect, useMemo, useState } from "react";
import { motion } from "framer-motion";
import {
  PawPrint,
  Stethoscope,
  Syringe,
  Scissors,
  HeartPulse,
  Bath,
  PhoneCall,
  ShieldCheck,
  Star,
  MapPin,
  Clock,
  ChevronRight,
  Quote,
} from "lucide-react";

/**
 * Landing page for a veterinary hospital website
 * Framework: React + Tailwind + (optional) shadcn/ui
 * Route: "/"
 *
 * Notes for integration:
 * - Hook up the Login and Sign up buttons to your existing auth routes ("/login", "/signup").
 * - Replace placeholder images with your assets (team photo, clinic photo, etc.).
 * - Connect the "Book Appointment" CTA to your actual appointment flow (e.g., "/appointments/new").
 * - If using a router (React Router / Next / TanStack), replace <a> with <Link> as needed.
 */

export default function LandingPage() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-white via-slate-50 to-white text-slate-800">
      <SiteHeader />
      <main id="main" className="relative">
        <Hero />
        <TrustBar />
        <Services />
        <HowItWorks />
        <Highlights />
        <Testimonials />
        <FAQ />
        <CTA />
      </main>
      <SiteFooter />
    </div>
  );
}

function Container({
  children,
  className = "",
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div
      className={`mx-auto w-full max-w-7xl px-4 sm:px-6 lg:px-8 ${className}`}
    >
      {children}
    </div>
  );
}

function SiteHeader() {
  const [open, setOpen] = useState(false);
  return (
    <header className="sticky top-0 z-40 backdrop-blur supports-[backdrop-filter]:bg-white/70 border-b border-slate-200/60">
      <Container className="flex h-16 items-center justify-between">
        <a href="/" className="flex items-center gap-2 group">
          <PawPrint
            className="h-6 w-6 text-indigo-600 group-hover:scale-110 transition"
            aria-hidden
          />
          <span className="font-semibold tracking-tight">
            VetCare+ Hospital
          </span>
        </a>

        <nav className="hidden md:flex items-center gap-8 text-sm">
          <a className="hover:text-indigo-700" href="#services">
            Services
          </a>
          <a className="hover:text-indigo-700" href="#doctors">
            Doctors
          </a>
          <a className="hover:text-indigo-700" href="#pricing">
            Pricing
          </a>
          <a className="hover:text-indigo-700" href="#faq">
            FAQs
          </a>
          <a className="hover:text-indigo-700" href="#contact">
            Contact
          </a>
        </nav>

        <div className="hidden md:flex items-center gap-3">
          <a
            href="/login"
            className="px-4 py-2 text-sm font-medium rounded-xl hover:bg-slate-100 focus:outline-none focus-visible:ring focus-visible:ring-indigo-500"
          >
            Log in
          </a>
          <a
            href="/register"
            className="px-4 py-2 text-sm font-semibold rounded-xl bg-indigo-600 text-white hover:bg-indigo-700 focus:outline-none focus-visible:ring focus-visible:ring-indigo-500"
          >
            Sign up
          </a>
        </div>

        {/* Mobile menu button */}
        <button
          className="md:hidden inline-flex items-center justify-center rounded-lg p-2 hover:bg-slate-100 focus-visible:ring focus-visible:ring-indigo-500"
          aria-label="Toggle menu"
          aria-expanded={open}
          onClick={() => setOpen(!open)}
        >
          <svg
            className="h-6 w-6"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
          >
            {open ? (
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M6 18L18 6M6 6l12 12"
              />
            ) : (
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M4 6h16M4 12h16M4 18h16"
              />
            )}
          </svg>
        </button>
      </Container>

      {/* Mobile menu */}
      {open && (
        <div className="md:hidden border-t border-slate-200/70 bg-white/80 backdrop-blur">
          <Container className="py-4 flex flex-col gap-2">
            <a className="py-2" href="#services" onClick={() => setOpen(false)}>
              Services
            </a>
            <a className="py-2" href="#doctors" onClick={() => setOpen(false)}>
              Doctors
            </a>
            <a className="py-2" href="#pricing" onClick={() => setOpen(false)}>
              Pricing
            </a>
            <a className="py-2" href="#faq" onClick={() => setOpen(false)}>
              FAQs
            </a>
            <a className="py-2" href="#contact" onClick={() => setOpen(false)}>
              Contact
            </a>
            <div className="mt-2 flex gap-2">
              <a
                href="/login"
                className="flex-1 rounded-xl border border-slate-200 py-2 text-center"
              >
                Log in
              </a>
              <a
                href="/register"
                className="flex-1 rounded-xl bg-indigo-600 text-white py-2 text-center"
              >
                Sign up
              </a>
            </div>
          </Container>
        </div>
      )}
    </header>
  );
}

function Hero() {
  return (
    <section className="relative overflow-hidden">
      {/* decorative background */}
      <div aria-hidden className="pointer-events-none absolute inset-0 -z-10">
        <div className="absolute -right-24 -top-24 h-72 w-72 rounded-full bg-indigo-200/40 blur-3xl" />
        <div className="absolute left-1/4 top-1/3 h-72 w-72 rounded-full bg-blue-200/40 blur-3xl" />
      </div>

      <Container className="grid lg:grid-cols-2 items-center gap-10 lg:gap-16 py-14 lg:py-24">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="text-center lg:text-left"
        >
          <span className="inline-flex items-center gap-2 rounded-full bg-indigo-50 px-3 py-1 text-xs font-medium text-indigo-700">
            <ShieldCheck className="h-4 w-4" /> 24/7 Emergency Care
          </span>
          <h1 className="mt-4 text-3xl sm:text-4xl lg:text-5xl font-extrabold tracking-tight">
            Compassionate care for your pets, powered by modern medicine.
          </h1>
          <p className="mt-4 text-slate-600 text-base sm:text-lg">
            From routine check‑ups to advanced surgery, our experienced vets
            keep tails wagging and hearts purring.
          </p>
          <div className="mt-6 flex flex-col sm:flex-row items-center gap-3 sm:gap-4 justify-center lg:justify-start">
            <a
              href="/appointments/new"
              className="inline-flex items-center gap-2 rounded-2xl bg-indigo-600 px-5 py-3 font-semibold text-white hover:bg-indigo-700 focus-visible:ring focus-visible:ring-indigo-500"
            >
              Book appointment <ChevronRight className="h-4 w-4" />
            </a>
            <a
              href="#services"
              className="inline-flex items-center gap-2 rounded-2xl border border-slate-300 px-5 py-3 font-medium hover:bg-white"
            >
              Explore services
            </a>
          </div>

          <ul className="mt-6 flex flex-wrap items-center justify-center lg:justify-start gap-x-6 gap-y-2 text-sm text-slate-600">
            <li className="inline-flex items-center gap-2">
              <Star className="h-4 w-4" /> 4.9/5 rating
            </li>
            <li className="inline-flex items-center gap-2">
              <Clock className="h-4 w-4" /> Same‑day slots
            </li>
            <li className="inline-flex items-center gap-2">
              <MapPin className="h-4 w-4" /> Colombo 03
            </li>
          </ul>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, scale: 0.98 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.6, delay: 0.1 }}
          className="relative"
        >
          <div className="aspect-[4/3] w-full overflow-hidden rounded-3xl shadow-xl ring-1 ring-slate-200">
            {/* Replace this with a real clinic/team photo */}
            <img
              src="https://images.unsplash.com/photo-1554224155-6726b3ff858f?q=80&w=1600&auto=format&fit=crop"
              alt="Vet examining a golden retriever on a table"
              className="h-full w-full object-cover"
              loading="eager"
            />
          </div>
          <div className="absolute -bottom-5 -left-5 rounded-2xl bg-white shadow-lg ring-1 ring-slate-200 p-4 flex items-center gap-3">
            <Stethoscope className="h-6 w-6 text-indigo-600" />
            <span className="text-sm font-medium">AAHA‑aligned protocols</span>
          </div>
        </motion.div>
      </Container>
    </section>
  );
}

function TrustBar() {
  const stats = [
    { label: "Happy patients", value: "12k+" },
    { label: "Years caring", value: "10+" },
    { label: "Emergency response", value: "< 10 min" },
    { label: "Avg. rating", value: "4.9/5" },
  ];
  return (
    <section aria-label="Key stats" className="py-8">
      <Container>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-6 rounded-2xl bg-white p-6 ring-1 ring-slate-200">
          {stats.map((s) => (
            <div key={s.label} className="text-center">
              <div className="text-2xl font-bold tracking-tight">{s.value}</div>
              <div className="text-xs text-slate-500 mt-1">{s.label}</div>
            </div>
          ))}
        </div>
      </Container>
    </section>
  );
}

function Services() {
  const items = [
    {
      icon: Stethoscope,
      title: "General Check‑ups",
      desc: "Comprehensive wellness exams for all ages.",
    },
    {
      icon: Syringe,
      title: "Vaccinations",
      desc: "Core & lifestyle vaccines with reminders.",
    },
    {
      icon: HeartPulse,
      title: "Diagnostics",
      desc: "On‑site labs, imaging & ECG for quick answers.",
    },
    {
      icon: Scissors,
      title: "Surgery",
      desc: "Spay/neuter and soft‑tissue procedures with modern anesthesia.",
    },
    {
      icon: Bath,
      title: "Grooming",
      desc: "Bathing, nail trims & dental cleaning.",
    },
    {
      icon: PhoneCall,
      title: "24/7 Emergency",
      desc: "Rapid triage and stabilization anytime.",
    },
  ];
  return (
    <section id="services" className="scroll-mt-20 py-16">
      <Container>
        <div className="mb-10 text-center">
          <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">
            Services built around comfort & safety
          </h2>
          <p className="mt-2 text-slate-600">
            Everything your pet needs under one roof.
          </p>
        </div>

        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {items.map(({ icon: Icon, title, desc }) => (
            <motion.div
              key={title}
              initial={{ opacity: 0, y: 10 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true, margin: "-50px" }}
              transition={{ duration: 0.4 }}
              className="group rounded-2xl bg-white p-6 ring-1 ring-slate-200 hover:shadow-lg"
            >
              <div className="flex items-center gap-3">
                <span className="rounded-xl bg-indigo-50 p-2 text-indigo-700">
                  <Icon className="h-5 w-5" />
                </span>
                <h3 className="font-semibold">{title}</h3>
              </div>
              <p className="mt-3 text-sm text-slate-600">{desc}</p>
              <a
                href="/appointments/new"
                className="mt-4 inline-flex items-center gap-1 text-sm font-medium text-indigo-700 hover:underline"
              >
                Book now <ChevronRight className="h-4 w-4" />
              </a>
            </motion.div>
          ))}
        </div>
      </Container>
    </section>
  );
}

function HowItWorks() {
  const steps = [
    {
      title: "Tell us about your pet",
      desc: "Create an account and add pet details in minutes.",
      badge: "Step 1",
    },
    {
      title: "Pick a service & time",
      desc: "See live availability and choose what works for you.",
      badge: "Step 2",
    },
    {
      title: "Check in & relax",
      desc: "Our team keeps you updated from check‑in to checkout.",
      badge: "Step 3",
    },
  ];
  return (
    <section className="py-16 bg-slate-50/60">
      <Container>
        <div className="mb-10 text-center">
          <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">
            Booking is easy
          </h2>
          <p className="mt-2 text-slate-600">
            Three simple steps to great care.
          </p>
        </div>
        <div className="grid gap-6 md:grid-cols-3">
          {steps.map((s, i) => (
            <motion.div
              key={s.title}
              initial={{ opacity: 0, y: 10 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.4, delay: i * 0.06 }}
              className="rounded-2xl bg-white p-6 ring-1 ring-slate-200"
            >
              <span className="inline-flex items-center rounded-full bg-indigo-50 px-2 py-1 text-xs font-medium text-indigo-700">
                {s.badge}
              </span>
              <h3 className="mt-3 font-semibold">{s.title}</h3>
              <p className="mt-2 text-sm text-slate-600">{s.desc}</p>
            </motion.div>
          ))}
        </div>
      </Container>
    </section>
  );
}

function Highlights() {
  const perks = [
    {
      title: "Fear‑Free handling",
      desc: "Low‑stress techniques for anxious pets.",
    },
    {
      title: "Transparent pricing",
      desc: "Up‑front estimates and option bundles.",
    },
    {
      title: "Digital records",
      desc: "All visits and labs in your secure portal.",
    },
    { title: "Follow‑up care", desc: "Post‑visit check‑ins & reminders." },
  ];
  return (
    <section id="doctors" className="py-16">
      <Container>
        <div className="grid lg:grid-cols-2 gap-10 items-center">
          <div>
            <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">
              Your pet’s health team, on your team
            </h2>
            <p className="mt-3 text-slate-600">
              Our veterinarians combine deep experience with genuine love for
              animals. Expect clear communication, tailored care plans, and a
              friendly clinic vibe.
            </p>
            <ul className="mt-6 grid sm:grid-cols-2 gap-4">
              {perks.map((p) => (
                <li
                  key={p.title}
                  className="rounded-2xl bg-white p-5 ring-1 ring-slate-200"
                >
                  <h3 className="font-semibold">{p.title}</h3>
                  <p className="mt-1 text-sm text-slate-600">{p.desc}</p>
                </li>
              ))}
            </ul>
          </div>

          <div className="aspect-[4/3] w-full overflow-hidden rounded-3xl shadow-xl ring-1 ring-slate-200">
            {/* Replace with your team/doctor image */}
            <img
              src="https://images.unsplash.com/photo-1559757175-08fda9f1e9f8?q=80&w=1600&auto=format&fit=crop"
              alt="Friendly veterinary team with a cat"
              className="h-full w-full object-cover"
            />
          </div>
        </div>
      </Container>
    </section>
  );
}

function Testimonials() {
  const items = [
    {
      name: "Sahan & " + "Bella",
      quote:
        "The booking was smooth and the vets were so gentle. Bella actually likes going now!",
    },
    {
      name: "Anika & Max",
      quote:
        "Transparent pricing and clear updates. We felt taken care of throughout.",
    },
    {
      name: "Ravi & Ginger",
      quote: "Emergency team responded in minutes — absolute lifesavers.",
    },
  ];

  const [index, setIndex] = useState(0);
  useEffect(() => {
    const id = setInterval(() => setIndex((i) => (i + 1) % items.length), 4500);
    return () => clearInterval(id);
  }, [items.length]);

  return (
    <section className="py-16 bg-slate-50/60">
      <Container>
        <div className="mb-8 text-center">
          <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">
            What pet parents say
          </h2>
          <p className="mt-2 text-slate-600">Real words from happy tails.</p>
        </div>

        <div className="relative mx-auto max-w-3xl">
          {items.map((t, i) => (
            <motion.blockquote
              key={t.name + i}
              initial={{ opacity: 0, y: 10 }}
              animate={{
                opacity: i === index ? 1 : 0,
                y: i === index ? 0 : 10,
              }}
              transition={{ duration: 0.5 }}
              className={`absolute inset-0 ${
                i === index ? "pointer-events-auto" : "pointer-events-none"
              } rounded-3xl bg-white p-8 ring-1 ring-slate-200`}
            >
              <Quote className="h-6 w-6 text-indigo-600" />
              <p className="mt-3 text-slate-700">{t.quote}</p>
              <footer className="mt-4 text-sm font-medium text-slate-600">
                — {t.name}
              </footer>
            </motion.blockquote>
          ))}
          <div className="invisible">
            {/* layout spacer for absolute quotes */}
            <blockquote className="rounded-3xl bg-white p-8 ring-1 ring-slate-200">
              Placeholder
            </blockquote>
          </div>
        </div>
      </Container>
    </section>
  );
}

function FAQ() {
  const faqs = [
    {
      q: "Do you take walk‑ins?",
      a: "We recommend booking online to minimize wait times, but we do accept urgent walk‑ins.",
    },
    {
      q: "What animals do you treat?",
      a: "Dogs & cats primarily. Call us for exotics and we’ll advise availability.",
    },
    {
      q: "Payment options?",
      a: "Cash, card, and most digital wallets. Clear estimates given before procedures.",
    },
  ];
  return (
    <section id="faq" className="py-16">
      <Container>
        <div className="mb-8 text-center">
          <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">
            FAQs
          </h2>
          <p className="mt-2 text-slate-600">
            Quick answers to common questions.
          </p>
        </div>
        <div className="mx-auto max-w-3xl divide-y divide-slate-200 overflow-hidden rounded-2xl bg-white ring-1 ring-slate-200">
          {faqs.map((item, idx) => (
            <details key={idx} className="group">
              <summary className="flex cursor-pointer list-none items-center justify-between px-6 py-4 font-medium">
                {item.q}
                <svg
                  className="h-5 w-5 transition-transform group-open:rotate-180"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                >
                  <path
                    fillRule="evenodd"
                    d="M5.23 7.21a.75.75 0 011.06.02L10 10.94l3.71-3.71a.75.75 0 011.08 1.04l-4.25 4.25a.75.75 0 01-1.06 0L5.21 8.27a.75.75 0 01.02-1.06z"
                    clipRule="evenodd"
                  />
                </svg>
              </summary>
              <div className="px-6 pb-4 text-sm text-slate-600">{item.a}</div>
            </details>
          ))}
        </div>
      </Container>
    </section>
  );
}

function CTA() {
  return (
    <section id="pricing" className="py-16">
      <Container>
        <div className="rounded-3xl bg-gradient-to-r from-indigo-600 to-blue-600 p-1">
          <div className="rounded-3xl bg-white p-8 sm:p-10 grid lg:grid-cols-2 gap-6 items-center">
            <div>
              <h3 className="text-2xl font-bold tracking-tight">
                Ready to start?
              </h3>
              <p className="mt-2 text-slate-600">
                Create an account to manage pets, book visits, and access
                records 24/7.
              </p>
            </div>
            <div className="flex flex-col sm:flex-row gap-3 sm:justify-end">
              <a
                href="/register"
                className="rounded-2xl bg-indigo-600 px-5 py-3 text-white font-semibold hover:bg-indigo-700"
              >
                Sign up
              </a>
              <a
                href="/login"
                className="rounded-2xl border border-slate-300 px-5 py-3 font-medium hover:bg-white"
              >
                Log in
              </a>
            </div>
          </div>
        </div>
      </Container>
    </section>
  );
}

function SiteFooter() {
  return (
    <footer id="contact" className="border-t border-slate-200/70 py-10">
      <Container className="grid gap-8 md:grid-cols-4">
        <div>
          <div className="flex items-center gap-2">
            <PawPrint className="h-6 w-6 text-indigo-600" />
            <span className="font-semibold">VetCare+ Hospital</span>
          </div>
          <p className="mt-2 text-sm text-slate-600">
            Caring for pets across Colombo and beyond.
          </p>
        </div>
        <div>
          <h4 className="font-semibold">Visit us</h4>
          <address className="not-italic mt-2 text-sm text-slate-600">
            123 Galle Rd,
            <br />
            Colombo 03, Sri Lanka
          </address>
        </div>
        <div>
          <h4 className="font-semibold">Contact</h4>
          <p className="mt-2 text-sm text-slate-600">
            Hotline: +94 11 234 5678
            <br />
            Email: care@vetcareplus.lk
          </p>
        </div>
        <div>
          <h4 className="font-semibold">Hours</h4>
          <p className="mt-2 text-sm text-slate-600">
            Mon–Sat 8am–8pm
            <br />
            Sun 9am–5pm
            <br />
            Emergency 24/7
          </p>
        </div>
      </Container>
      <Container className="mt-8 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-slate-500">
        <p>
          © {new Date().getFullYear()} VetCare+ Hospital. All rights reserved.
        </p>
        <div className="flex items-center gap-4">
          <a href="#" className="hover:text-slate-700">
            Privacy
          </a>
          <a href="#" className="hover:text-slate-700">
            Terms
          </a>
          <a href="#main" className="hover:text-slate-700">
            Back to top
          </a>
        </div>
      </Container>
    </footer>
  );
}
