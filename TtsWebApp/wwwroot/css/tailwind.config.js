// Tailwind CSS Configuration for VoiceGen
// This configuration extends Tailwind with custom colors and fonts

tailwind.config = {
    darkMode: "class",
    theme: {
        extend: {
            colors: {
                "primary": "#5b13ec",
                "background-light": "#f6f6f8",
                "background-dark": "#161022",
            },
            fontFamily: {
                "display": ["Space Grotesk", "sans-serif"]
            },
            borderRadius: {
                "DEFAULT": "0.25rem",
                "lg": "0.5rem",
                "xl": "0.75rem",
                "full": "9999px"
            },
        },
    },
}
