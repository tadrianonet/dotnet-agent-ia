using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using LangChain.Chains.LLM;
using LangChain.Prompts;
using LangChain.Providers.OpenAI.Predefined;
using LangChain.Schema;

DotNetEnv.Env.Load();
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Erro: OPENAI_API_KEY não definida.");
    return;
}

static string ExtractTextFromPdf(string filePath)
{
    StringBuilder text = new StringBuilder();
    using (PdfReader reader = new PdfReader(filePath))
    using (PdfDocument pdfDoc = new PdfDocument(reader))
    {
        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            text.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
        }
    }
    return text.ToString();
}

string pdfPath = "profile.pdf"; 

if (!File.Exists(pdfPath))
{
    Console.WriteLine("Erro: O arquivo PDF não foi encontrado.");
    return;
}

string candidateResume = ExtractTextFromPdf(pdfPath);

var prompt = new PromptTemplate(new PromptTemplateInput(
    template: """
    Você é um especialista em Recursos Humanos, especializado na contratação de Tech Leads.
    Avalie o seguinte currículo e determine se o candidato está apto para uma posição de Tech Lead.

    Resumo do Candidato:
    {resume}

    Baseando-se nas habilidades técnicas, experiência de liderança e competências descritas, 
    forneça uma análise detalhada indicando se o candidato é adequado para a vaga de Tech Lead. Justifique sua resposta.
    """,
    inputVariables: ["resume"]));

var llm = new OpenAiLatestFastChatModel(apiKey!);

var chain = new LlmChain(new LlmChainInput(llm, prompt));

var result = await chain.CallAsync(new ChainValues(new Dictionary<string, object>
{
    { "resume", candidateResume }
}));

Console.WriteLine("Resultado da Análise do Candidato:");
Console.WriteLine(result.Value["text"]);
