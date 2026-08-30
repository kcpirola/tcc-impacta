import api from "./api";
import * as emailService from "./emailService";

jest.mock("./api");

describe("emailService", () => {
  afterEach(() => {
    jest.clearAllMocks();
  });

  it("classifyEmail posts to /emails/classify and returns the response data", async () => {
    const responseData = { classification: "phishing", confidence: 91.2 };
    api.post.mockResolvedValue({ data: responseData });

    const result = await emailService.classifyEmail({
      subject: "Urgente",
      body: "Clique aqui",
      sender: "attacker@malicious.tk",
    });

    expect(api.post).toHaveBeenCalledWith("/emails/classify", expect.objectContaining({ subject: "Urgente" }));
    expect(result).toEqual(responseData);
  });

  it("classifyEmailsBatch posts the full list and returns the response data", async () => {
    const emails = [{ subject: "A" }, { subject: "B" }];
    const responseData = [{ classification: "safe" }, { classification: "phishing" }];
    api.post.mockResolvedValue({ data: responseData });

    const result = await emailService.classifyEmailsBatch(emails);

    expect(api.post).toHaveBeenCalledWith("/emails/classify-batch", emails);
    expect(result).toEqual(responseData);
  });

  it("submitFeedback posts emailId and feedback", async () => {
    api.post.mockResolvedValue({ data: { message: "ok" } });

    await emailService.submitFeedback(42, "correct");

    expect(api.post).toHaveBeenCalledWith("/emails/feedback", { emailId: 42, feedback: "correct" });
  });

  it("getEmailsByClassification requests the correct endpoint with pagination params", async () => {
    const responseData = [{ id: 1 }];
    api.get.mockResolvedValue({ data: responseData });

    const result = await emailService.getEmailsByClassification("phishing", 10, 5);

    expect(api.get).toHaveBeenCalledWith("/emails/by-classification/phishing", {
      params: { skip: 10, take: 5 },
    });
    expect(result).toEqual(responseData);
  });
});
