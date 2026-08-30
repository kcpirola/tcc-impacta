import { render, screen, fireEvent } from "@testing-library/react";
import EmailTable from "./EmailTable";

const sampleEmails = [
  {
    id: 1,
    sender: "attacker@malicious.tk",
    subject: "URGENTE: verifique sua conta",
    classification: "phishing",
    confidence: 92.4,
    receivedDate: "2026-08-30T10:00:00Z",
  },
];

describe("EmailTable", () => {
  it("renders an empty state message when there are no emails", () => {
    render(<EmailTable emails={[]} onFeedback={jest.fn()} />);
    expect(screen.getByText("Nenhum e-mail encontrado.")).toBeInTheDocument();
  });

  it("renders a row per email with sender, subject and classification", () => {
    render(<EmailTable emails={sampleEmails} onFeedback={jest.fn()} />);

    expect(screen.getByText("attacker@malicious.tk")).toBeInTheDocument();
    expect(screen.getByText("URGENTE: verifique sua conta")).toBeInTheDocument();
    expect(screen.getByText("Phishing")).toBeInTheDocument();
  });

  it("calls onFeedback with the email id and feedback type when a button is clicked", () => {
    const handleFeedback = jest.fn();
    render(<EmailTable emails={sampleEmails} onFeedback={handleFeedback} />);

    fireEvent.click(screen.getByTitle("Classificação correta"));

    expect(handleFeedback).toHaveBeenCalledWith(1, "correct");
  });
});
