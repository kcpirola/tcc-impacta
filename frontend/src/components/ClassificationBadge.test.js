import { render, screen } from "@testing-library/react";
import ClassificationBadge from "./ClassificationBadge";

describe("ClassificationBadge", () => {
  it.each([
    ["safe", "Seguro"],
    ["suspicious", "Suspeito"],
    ["phishing", "Phishing"],
  ])("renders the Portuguese label for classification=%s", (classification, expectedLabel) => {
    render(<ClassificationBadge classification={classification} />);
    expect(screen.getByText(expectedLabel)).toBeInTheDocument();
  });

  it("falls back to the raw value for unknown classifications", () => {
    render(<ClassificationBadge classification="unknown" />);
    expect(screen.getByText("unknown")).toBeInTheDocument();
  });
});
