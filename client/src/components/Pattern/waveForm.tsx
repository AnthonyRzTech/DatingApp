import { theme } from "@/components/theme";
import "./waveMoves.css";

interface WaveFormProps {
  children?: React.ReactNode;
  c1?: string;
  c2?: string;
  c3?: string;
  c4?: string;
  animation?: "none" | "waveMoves" | "scroll";
}

export const WaveForm: React.FC<WaveFormProps> = ({
  children,
  c1 = theme.palette.background.default,
  c2 = theme.palette.secondary.main,
  c3 = "#0008",
  c4 = "#0000",
  animation = "waveMoves",
}) => {
  const dynamicStyle = {
    minHeight: "100vh",
    minWidth: "100vw",
    "--s": "100px",
    "--c1": c1,
    "--c2": c2,
    "--c3": c3,
    "--c4": c4,
    "--_g": `var(--c2) 4% 14%, var(--c1) 14% 24%, var(--c2) 22% 34%, var(--c1) 34% 44%, var(--c2) 44% 56%, var(--c1) 56% 66%, var(--c2) 66% 76%, var(--c1) 76% 86%, var(--c2) 86% 96%`,
    background:
      "radial-gradient(100% 100% at 100% 0, var(--c1) 4%, var(--_g), var(--c3) 96%, var(--c4)), radial-gradient(100% 100% at 0 100%, var(--c4), var(--c3) 4%, var(--_g), var(--c1) 96%) var(--c1)",
    backgroundSize: "var(--s) var(--s)",
    animation:
      animation !== "none"
        ? `${animation} ${
            animation === "scroll" ? "3s" : "8s"
          } infinite ease-in-out`
        : "none",
  } as React.CSSProperties;

  return <div style={dynamicStyle}>{children}</div>;
};
