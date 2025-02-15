using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using LangChain.Chains.LLM;
using LangChain.Prompts;
using LangChain.Providers.OpenAI.Predefined;
using LangChain.Schema;

// Carregar as variáveis de ambiente
DotNetEnv.Env.Load();
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Erro: OPENAI_API_KEY não definida.");
    return;
}

// Inicializa o modelo LLM da OpenAI
var llm = new OpenAiLatestFastChatModel(apiKey!);

// Função para extrair texto do PDF
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

// Caminho do arquivo PDF
string pdfPath = "profile.pdf"; // Substitua pelo caminho real do seu arquivo

if (!File.Exists(pdfPath))
{
    Console.WriteLine("Erro: O arquivo PDF não foi encontrado.");
    return;
}

// Lê o conteúdo do PDF
string candidateResume = ExtractTextFromPdf(pdfPath);

// Criação do prompt especializado para avaliação
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

// Criando a cadeia LLM para análise do currículo
var chain = new LlmChain(new LlmChainInput(llm, prompt));

// Executando a análise
var result = await chain.CallAsync(new ChainValues(new Dictionary<string, object>
{
    { "resume", candidateResume }
}));

// Exibe o resultado da avaliação
Console.WriteLine("Resultado da Análise do Candidato:");
Console.WriteLine(result.Value["text"]);
