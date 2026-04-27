using StudyENEM.API.Models;

namespace StudyENEM.API.Data;

public static class Seed
{
    public static void Apply(AppDbContext db)
    {
        if (db.Questions.Any()) return;

        var questions = new List<Question>
        {
            new() {
                Year = 2023, Area = "Ciências da Natureza", Subject = "Física",
                Statement = "Um objeto é lançado verticalmente para cima com velocidade inicial de 20 m/s. Considerando g = 10 m/s², qual é a altura máxima atingida?",
                OptionA = "10 m", OptionB = "20 m", OptionC = "30 m", OptionD = "40 m", OptionE = "50 m",
                CorrectOption = 'B'
            },
            new() {
                Year = 2023, Area = "Ciências da Natureza", Subject = "Química",
                Statement = "Qual é o número de mols de átomos de hidrogênio presentes em 36 g de água (H₂O)? Massa molar: H=1, O=16.",
                OptionA = "1 mol", OptionB = "2 mol", OptionC = "3 mol", OptionD = "4 mol", OptionE = "6 mol",
                CorrectOption = 'D'
            },
            new() {
                Year = 2023, Area = "Ciências da Natureza", Subject = "Biologia",
                Statement = "A fotossíntese ocorre em dois estágios principais. Qual das alternativas descreve corretamente esses estágios?",
                OptionA = "Glicólise e ciclo de Krebs",
                OptionB = "Reações de luz e ciclo de Calvin",
                OptionC = "Fermentação e respiração celular",
                OptionD = "Mitose e meiose",
                OptionE = "Transcriçao e tradução",
                CorrectOption = 'B'
            },
            new() {
                Year = 2023, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Statement = "Uma progressão aritmética tem primeiro termo a₁ = 3 e razão r = 4. Qual é o décimo termo?",
                OptionA = "35", OptionB = "39", OptionC = "43", OptionD = "47", OptionE = "51",
                CorrectOption = 'B'
            },
            new() {
                Year = 2023, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Statement = "Qual é a solução da inequação 2x - 6 > 0?",
                OptionA = "x < 3", OptionB = "x > 3", OptionC = "x = 3", OptionD = "x < -3", OptionE = "x > -3",
                CorrectOption = 'B'
            },
            new() {
                Year = 2023, Area = "Linguagens, Códigos e suas Tecnologias", Subject = "Língua Portuguesa",
                Statement = "Assinale a alternativa em que todas as palavras estão corretamente acentuadas segundo as normas do Acordo Ortográfico de 2009.",
                OptionA = "herói, júri, bônus",
                OptionB = "héroe, júri, bonus",
                OptionC = "herói, juri, bônus",
                OptionD = "heroi, júri, bônus",
                OptionE = "herói, júri, bonus",
                CorrectOption = 'A'
            },
            new() {
                Year = 2023, Area = "Linguagens, Códigos e suas Tecnologias", Subject = "Literatura",
                Statement = "O Realismo brasileiro do século XIX é marcado pela análise psicológica dos personagens. Qual obra de Machado de Assis exemplifica melhor essa característica?",
                OptionA = "Iracema",
                OptionB = "Dom Casmurro",
                OptionC = "O Guarani",
                OptionD = "A Moreninha",
                OptionE = "Inocência",
                CorrectOption = 'B'
            },
            new() {
                Year = 2023, Area = "Ciências Humanas e suas Tecnologias", Subject = "História",
                Statement = "A Revolução Industrial iniciada na Inglaterra no século XVIII trouxe profundas transformações sociais. Qual das alternativas representa uma consequência direta desse processo?",
                OptionA = "Fortalecimento do sistema feudal",
                OptionB = "Crescimento do proletariado urbano",
                OptionC = "Declínio do capitalismo comercial",
                OptionD = "Redução da jornada de trabalho",
                OptionE = "Diminuição da produção agrícola",
                CorrectOption = 'B'
            },
            new() {
                Year = 2023, Area = "Ciências Humanas e suas Tecnologias", Subject = "Geografia",
                Statement = "O fenômeno El Niño provoca alterações climáticas em diversas regiões do mundo. No Brasil, qual região é mais afetada pela redução das chuvas durante esse fenômeno?",
                OptionA = "Amazônia",
                OptionB = "Cerrado",
                OptionC = "Nordeste",
                OptionD = "Sul",
                OptionE = "Pantanal",
                CorrectOption = 'C'
            },
            new() {
                Year = 2022, Area = "Ciências da Natureza", Subject = "Física",
                Statement = "Um capacitor plano tem capacitância C = 4 µF e é conectado a uma bateria de 12 V. Qual é a carga armazenada no capacitor?",
                OptionA = "16 µC", OptionB = "24 µC", OptionC = "36 µC", OptionD = "48 µC", OptionE = "56 µC",
                CorrectOption = 'D'
            },
            new() {
                Year = 2022, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Statement = "A função f(x) = x² - 4x + 3 tem raízes nos pontos:",
                OptionA = "x = 1 e x = 3", OptionB = "x = -1 e x = -3", OptionC = "x = 2 e x = 2",
                OptionD = "x = 0 e x = 4", OptionE = "x = 1 e x = -3",
                CorrectOption = 'A'
            },
            new() {
                Year = 2022, Area = "Ciências Humanas e suas Tecnologias", Subject = "Filosofia",
                Statement = "Para Immanuel Kant, o imperativo categórico pode ser resumido como:",
                OptionA = "Age de tal modo que trates a humanidade sempre como um fim e nunca como um meio",
                OptionB = "O fim justifica os meios",
                OptionC = "A virtude é o caminho do meio entre dois extremos",
                OptionD = "Só sei que nada sei",
                OptionE = "Penso, logo existo",
                CorrectOption = 'A'
            },
            new() {
                Year = 2022, Area = "Linguagens, Códigos e suas Tecnologias", Subject = "Inglês",
                Statement = "Choose the sentence in which the Present Perfect is used correctly:",
                OptionA = "I have went to the store yesterday",
                OptionB = "She has never been to Paris",
                OptionC = "They have saw that movie last week",
                OptionD = "He has go to school every day",
                OptionE = "We have ate lunch an hour ago",
                CorrectOption = 'B'
            },
            new() {
                Year = 2022, Area = "Ciências da Natureza", Subject = "Biologia",
                Statement = "O DNA contém a informação genética dos seres vivos. Qual é o processo pelo qual a informação do DNA é copiada para o RNA mensageiro?",
                OptionA = "Tradução",
                OptionB = "Replicação",
                OptionC = "Transcrição",
                OptionD = "Mitose",
                OptionE = "Recombinação",
                CorrectOption = 'C'
            },
            new() {
                Year = 2022, Area = "Ciências da Natureza", Subject = "Química",
                Statement = "A eletrólise da água produz quais substâncias nos eletrodos?",
                OptionA = "Hidrogênio no catodo e oxigênio no anodo",
                OptionB = "Oxigênio no catodo e hidrogênio no anodo",
                OptionC = "Hidrogênio em ambos os eletrodos",
                OptionD = "Oxigênio em ambos os eletrodos",
                OptionE = "Água oxigenada no catodo",
                CorrectOption = 'A'
            },
        };

        db.Questions.AddRange(questions);
        db.SaveChanges();
    }
}
