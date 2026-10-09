
(function () {
    function initMobileMenu() {
        const menuToggle = document.querySelector(".menu-toggle");
        const mainNav = document.querySelector(".main-nav");

        if (!menuToggle || !mainNav) {
            console.error("Mobile menu elements not found!", {
                menuToggle,
                mainNav
            });
            return;
        }

        function closeMenu() {
            mainNav.classList.remove("open");
            menuToggle.setAttribute("aria-expanded", "false");
            document.body.classList.remove("menu-open");
        }

        function toggleMenu(event) {
            event.stopPropagation();

            const isOpen = mainNav.classList.toggle("open");

            menuToggle.setAttribute("aria-expanded", String(isOpen));
            document.body.classList.toggle("menu-open", isOpen);
        }

        menuToggle.addEventListener("click", toggleMenu);

        mainNav.querySelectorAll("a").forEach(function (link) {
            link.addEventListener("click", closeMenu);
        });

        document.addEventListener("click", function (event) {
            if (
                mainNav.classList.contains("open") &&
                !mainNav.contains(event.target) &&
                !menuToggle.contains(event.target)
            ) {
                closeMenu();
            }
        });

        window.addEventListener("resize", function () {
            if (window.innerWidth > 900) {
                closeMenu();
            }
        });

        console.log("Techninxa mobile menu initialized successfully.");
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initMobileMenu);
    } else {
        initMobileMenu();
    }
})();
document.addEventListener("DOMContentLoaded", () => {
  const header = document.getElementById("siteHeader");
  const menuToggle = document.querySelector(".menu-toggle");
  const nav = document.querySelector(".main-nav");

  const updateHeader = () => {
    if (header) header.classList.toggle("scrolled", window.scrollY > 20);
  };
  updateHeader();
  window.addEventListener("scroll", updateHeader, { passive: true });

  if (menuToggle && nav) {
    menuToggle.addEventListener("click", () => {
      const open = nav.classList.toggle("open");
      menuToggle.setAttribute("aria-expanded", String(open));
      document.body.classList.toggle("menu-open", open);
    });

    nav.querySelectorAll("a").forEach(link => {
      link.addEventListener("click", () => {
        nav.classList.remove("open");
        menuToggle.setAttribute("aria-expanded", "false");
        document.body.classList.remove("menu-open");
      });
    });
  }

  const observer = new IntersectionObserver((entries, obs) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add("is-visible");
        obs.unobserve(entry.target);
      }
    });
  }, { threshold: 0.1 });

  document.querySelectorAll(".reveal").forEach(el => observer.observe(el));

  document.querySelectorAll("[data-count]").forEach(counter => {
    const target = Number(counter.dataset.count);
    const suffix = counter.dataset.suffix || "";
    let started = false;
    const counterObserver = new IntersectionObserver(entries => {
      if (!entries[0].isIntersecting || started) return;
      started = true;
      const start = performance.now();
      const duration = 1100;
      const tick = now => {
        const progress = Math.min((now - start) / duration, 1);
        const eased = 1 - Math.pow(1 - progress, 3);
        counter.textContent = Math.round(target * eased) + suffix;
        if (progress < 1) requestAnimationFrame(tick);
      };
      requestAnimationFrame(tick);
      counterObserver.disconnect();
    }, { threshold: .6 });
    counterObserver.observe(counter);
  });

  const form = document.getElementById("contactForm");
  const formNote = document.getElementById("formNote");
  if (form) {
    form.addEventListener("submit", event => {
      event.preventDefault();
      const name = form.elements.name.value.trim();
      if (!name) return;
      formNote.textContent = `Thanks ${name}! Your message is ready to be connected to a backend.`;
      form.reset();
    });
  }

  const filters = document.querySelectorAll(".filter");
  const cards = document.querySelectorAll(".library-card");
  if (filters.length && cards.length) {
    filters.forEach(filter => {
      filter.addEventListener("click", () => {
        filters.forEach(btn => btn.classList.remove("active"));
        filter.classList.add("active");
        const selected = filter.dataset.filter;
        cards.forEach(card => {
          const categories = card.dataset.category || "";
          card.classList.toggle("hidden", selected !== "all" && !categories.includes(selected));
        });
      });
    });
  }

  const projects = {
    "smart-water-tank": {
      category: "IoT / MOBILE",
      title: "Smart Water Tank",
      intro: "Connected water-level monitoring designed for simple, reliable visibility.",
      overviewTitle: "A connected system built around real-time visibility.",
      overview: "Smart Water Tank combines an ESP32 device, water-level sensing, connectivity and a mobile dashboard to make tank status easy to understand.",
      challenge: "The interface focuses on clear status information, simple actions and useful feedback even when connectivity changes.",
      type: "IoT / Internal Product",
      platform: "Android • IoT",
      tech: ["Flutter", "ESP32", "Firebase", "Wi-Fi", "Bluetooth", "REST API"],
      features: [
        ["01", "Live Monitoring", "View current tank level and volume in a focused mobile dashboard."],
        ["02", "Connectivity", "Use Wi-Fi with device communication designed around real-world conditions."],
        ["03", "Clear Status", "Make level, connection and system state easy to understand at a glance."],
        ["04", "Smart Device", "Connect sensing hardware with a practical software experience."]
      ],
      next: ["PetCall", "petcall"]
    },
    "petcall": {
      category: "ANDROID / MOBILE",
      title: "PetCall",
      intro: "A playful Android utility designed around voice recognition and sound playback.",
      overviewTitle: "A small mobile experience with a simple interaction loop.",
      overview: "PetCall listens for a configured nickname and can trigger a selected sound, creating a lightweight interaction for pet owners.",
      challenge: "The UI uses a friendly visual hierarchy, clear listening state and quick sound controls so the experience stays simple.",
      type: "Mobile Utility",
      platform: "Android",
      tech: ["Java", "Android XML", "SpeechRecognizer", "Audio", "Notifications"],
      features: [
        ["01", "Name Listening", "Listen for a configured pet nickname using Android speech recognition."],
        ["02", "Sound Library", "Choose from different sounds and audio responses."],
        ["03", "Background Mode", "Support an always-ready interaction through a foreground service."],
        ["04", "Visual Feedback", "Use clear state changes so listening and playback are easy to follow."]
      ],
      next: ["Smart Water Tank", "smart-water-tank"]
    },
    "task-manager": {
      category: "FLUTTER / MOBILE",
      title: "Task Manager",
      intro: "A focused task workflow for organizing work by status and priority.",
      overviewTitle: "A clean mobile workflow for everyday task management.",
      overview: "Task Manager organizes tasks into useful states such as new, progress, completed and canceled while keeping navigation simple.",
      challenge: "The interface was designed around quick status changes and an uncluttered mobile-first workflow.",
      type: "Productivity App",
      platform: "Flutter • Android / iOS",
      tech: ["Flutter", "Dart", "GetX", "Firebase", "Responsive UI"],
      features: [
        ["01", "Status Workflow", "Move tasks through a clear set of progress states."],
        ["02", "Fast Navigation", "Keep important actions close to the user's thumb."],
        ["03", "Reusable UI", "Use modular cards and components for consistent interaction."],
        ["04", "Responsive", "Adapt the experience across common mobile screen sizes."]
      ],
      next: ["PetCall", "petcall"]
    },
    "web-platform": {
      category: "WEB",
      title: "Digital Portal",
      intro: "A responsive web portal concept for services, data and user workflows.",
      overviewTitle: "A flexible foundation for modern web services.",
      overview: "Digital Portal brings dashboards, service modules and responsive layouts into one structured web experience.",
      challenge: "The visual system emphasizes hierarchy, readability and reusable components so additional modules can be added without redesigning the whole product.",
      type: "Web Platform",
      platform: "Web",
      tech: ["HTML", "CSS", "JavaScript", "REST API", "MySQL"],
      features: [
        ["01", "Dashboard", "Present key information in focused, scannable views."],
        ["02", "Modules", "Structure functionality into reusable service areas."],
        ["03", "Responsive", "Support desktop, tablet and mobile layouts."],
        ["04", "Data Ready", "Designed to connect cleanly with APIs and databases."]
      ],
      next: ["Smart Water Tank", "smart-water-tank"]
    },
    "iot-monitor": {
      category: "IOT / EMBEDDED",
      title: "IoT Monitor",
      intro: "An embedded monitoring concept connecting sensor telemetry to software.",
      overviewTitle: "Hardware and software working as one system.",
      overview: "IoT Monitor demonstrates a connected-device workflow where an ESP32 collects telemetry and a digital interface makes the data useful.",
      challenge: "The system is structured around stable device communication, readable status information and extensible sensor inputs.",
      type: "IoT Prototype",
      platform: "ESP32 • Web",
      tech: ["ESP32", "Arduino", "Wi-Fi", "HTTP", "JavaScript"],
      features: [
        ["01", "Telemetry", "Capture sensor readings directly from connected hardware."],
        ["02", "Device Status", "Expose online and operational state clearly."],
        ["03", "Network Ready", "Connect devices to a local or remote application layer."],
        ["04", "Expandable", "Keep the architecture open for additional sensors."]
      ],
      next: ["Digital Portal", "web-platform"]
    }
  };

  const projectPage = document.getElementById("projectPage");
  if (projectPage) {
    const params = new URLSearchParams(window.location.search);
    const id = params.get("id") || "smart-water-tank";
    const data = projects[id] || projects["smart-water-tank"];

    const setText = (selector, value) => {
      const el = document.querySelector(selector);
      if (el) el.textContent = value;
    };

    setText("#projectCategory", data.category);
    setText("#projectTitle", data.title);
    setText("#projectIntro", data.intro);
    setText("#projectOverviewTitle", data.overviewTitle);
    setText("#projectOverview", data.overview);
    setText("#projectChallenge", data.challenge);
    setText("#projectType", data.type);
    setText("#projectPlatform", data.platform);

    const tech = document.getElementById("projectTech");
    if (tech) tech.innerHTML = data.tech.map(item => `<span>${item}</span>`).join("");

    const features = document.getElementById("projectFeatures");
    if (features) {
      features.innerHTML = data.features.map(item => `
        <article class="feature-card reveal is-visible">
          <span class="feature-number">${item[0]}</span>
          <h3>${item[1]}</h3>
          <p>${item[2]}</p>
        </article>`).join("");
    }

    const nextTitle = document.getElementById("nextProjectTitle");
    const nextLink = document.getElementById("nextProjectLink");
    if (nextTitle) nextTitle.textContent = data.next[0];
    if (nextLink) nextLink.href = `project-view.html?id=${data.next[1]}`;

    document.title = `${data.title} — Techninxa`;
  }

  const year = document.getElementById("year");
  if (year) year.textContent = new Date().getFullYear();
});
const backToTop = document.getElementById("backToTop");

window.addEventListener("scroll", function () {

    if (window.scrollY > 400) {
        backToTop.classList.add("show");
    } else {
        backToTop.classList.remove("show");
    }

});

backToTop.addEventListener("click", function () {

    window.scrollTo({
        top: 0,
        behavior: "smooth"
    });

}); 


/* =========================================
   SMOOTH CUSTOM CURSOR
   ========================================= */

const cursorDot = document.querySelector(".cursor-dot");
const cursorRing = document.querySelector(".cursor-ring");

if (cursorDot && cursorRing) {

    let mouseX = window.innerWidth / 2;
    let mouseY = window.innerHeight / 2;

    let ringX = mouseX;
    let ringY = mouseY;

    document.addEventListener("mousemove", (e) => {

        mouseX = e.clientX;
        mouseY = e.clientY;

        // Small dot follows immediately
        cursorDot.style.left = `${mouseX}px`;
        cursorDot.style.top = `${mouseY}px`;

    });

    function animateCursor() {

        // Smooth delayed movement
        ringX += (mouseX - ringX) * 0.12;
        ringY += (mouseY - ringY) * 0.12;

        cursorRing.style.left = `${ringX}px`;
        cursorRing.style.top = `${ringY}px`;

        requestAnimationFrame(animateCursor);
    }

    animateCursor();


    /* =====================================
       HOVER EFFECT
       ===================================== */

    const interactiveElements = document.querySelectorAll(
        "a, button, input, textarea, select, .service-card, .project-feature-card"
    );

    interactiveElements.forEach((element) => {

        element.addEventListener("mouseenter", () => {
            document.body.classList.add("cursor-hover");
        });

        element.addEventListener("mouseleave", () => {
            document.body.classList.remove("cursor-hover");
        });

    });
}




document.addEventListener("DOMContentLoaded", () => {
    const menuToggle = document.querySelector(".menu-toggle");
    const mainNav = document.querySelector(".main-nav");

    if (!menuToggle || !mainNav) return;

    function closeMenu() {
        mainNav.classList.remove("open");
        menuToggle.setAttribute("aria-expanded", "false");
        document.body.classList.remove("menu-open");
    }

    menuToggle.addEventListener("click", (event) => {
        event.stopPropagation();

        const isOpen = mainNav.classList.toggle("open");

        menuToggle.setAttribute("aria-expanded", String(isOpen));
        document.body.classList.toggle("menu-open", isOpen);
    });

    mainNav.querySelectorAll("a").forEach((link) => {
        link.addEventListener("click", closeMenu);
    });

    document.addEventListener("click", (event) => {
        if (
            mainNav.classList.contains("open") &&
            !mainNav.contains(event.target) &&
            !menuToggle.contains(event.target)
        ) {
            closeMenu();
        }
    });

    window.addEventListener("resize", () => {
        if (window.innerWidth > 800) {
            closeMenu();
        }
    });
});