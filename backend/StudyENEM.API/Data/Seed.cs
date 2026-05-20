using StudyENEM.API.Models;

namespace StudyENEM.API.Data;

public static class Seed
{
    public static void Apply(AppDbContext db)
    {
        if (db.Questions.Any()) return;

        var questions = new List<Question>
        {
            // ── Ciências da Natureza ──────────────────────────────────────────
            new() {
                Year = 2023, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Física",
                Topic = "Cinemática", Difficulty = "média",
                Statement = "Um objeto é lançado verticalmente para cima com velocidade inicial de 20 m/s. Considerando g = 10 m/s², qual é a altura máxima atingida?",
                OptionA = "10 m", OptionB = "20 m", OptionC = "30 m", OptionD = "40 m", OptionE = "50 m",
                CorrectOption = 'B'
            },
            new() {
                Year = 2022, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Física",
                Topic = "Eletrostática", Difficulty = "difícil",
                Statement = "Um capacitor plano tem capacitância C = 4 µF e é conectado a uma bateria de 12 V. Qual é a carga armazenada no capacitor?",
                OptionA = "16 µC", OptionB = "24 µC", OptionC = "36 µC", OptionD = "48 µC", OptionE = "56 µC",
                CorrectOption = 'D'
            },
            new() {
                Year = 2021, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Física",
                Topic = "Termodinâmica", Difficulty = "média",
                Statement = "Um gás ideal sofre uma transformação isotérmica, mantendo temperatura constante. Se o volume dobra, a pressão:",
                OptionA = "Dobra", OptionB = "Quadruplica", OptionC = "Reduz à metade", OptionD = "Permanece constante", OptionE = "Reduz a um quarto",
                CorrectOption = 'C'
            },
            new() {
                Year = 2020, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Física",
                Topic = "Óptica", Difficulty = "fácil",
                Statement = "A velocidade da luz no vácuo é aproximadamente 3 × 10⁸ m/s. Ao penetrar em um meio com índice de refração n = 1,5, a velocidade da luz passa a ser:",
                OptionA = "1,0 × 10⁸ m/s", OptionB = "1,5 × 10⁸ m/s", OptionC = "2,0 × 10⁸ m/s", OptionD = "2,5 × 10⁸ m/s", OptionE = "4,5 × 10⁸ m/s",
                CorrectOption = 'C'
            },
            new() {
                Year = 2023, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Química",
                Topic = "Estequiometria", Difficulty = "média",
                Statement = "Qual é o número de mols de átomos de hidrogênio presentes em 36 g de água (H₂O)? Massa molar: H=1, O=16.",
                OptionA = "1 mol", OptionB = "2 mol", OptionC = "3 mol", OptionD = "4 mol", OptionE = "6 mol",
                CorrectOption = 'D'
            },
            new() {
                Year = 2022, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Química",
                Topic = "Eletroquímica", Difficulty = "média",
                Statement = "A eletrólise da água produz quais substâncias nos eletrodos?",
                OptionA = "Hidrogênio no catodo e oxigênio no anodo", OptionB = "Oxigênio no catodo e hidrogênio no anodo",
                OptionC = "Hidrogênio em ambos os eletrodos", OptionD = "Oxigênio em ambos os eletrodos", OptionE = "Água oxigenada no catodo",
                CorrectOption = 'A'
            },
            new() {
                Year = 2021, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Química",
                Topic = "Reações químicas", Difficulty = "fácil",
                Statement = "A reação entre o ácido clorídrico (HCl) e o hidróxido de sódio (NaOH) produz cloreto de sódio e água. Esse tipo de reação é classificado como:",
                OptionA = "Oxirredução", OptionB = "Combustão", OptionC = "Neutralização", OptionD = "Síntese", OptionE = "Decomposição",
                CorrectOption = 'C'
            },
            new() {
                Year = 2020, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Química",
                Topic = "Termoquímica", Difficulty = "difícil",
                Statement = "Uma reação exotérmica libera energia para o ambiente. Qual das alternativas descreve corretamente a variação de entalpia (ΔH) de uma reação exotérmica?",
                OptionA = "ΔH > 0", OptionB = "ΔH = 0", OptionC = "ΔH < 0", OptionD = "ΔH depende da temperatura", OptionE = "ΔH é indefinido",
                CorrectOption = 'C'
            },
            new() {
                Year = 2023, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Biologia",
                Topic = "Fisiologia vegetal", Difficulty = "fácil",
                Statement = "A fotossíntese ocorre em dois estágios principais. Qual das alternativas descreve corretamente esses estágios?",
                OptionA = "Glicólise e ciclo de Krebs", OptionB = "Reações de luz e ciclo de Calvin",
                OptionC = "Fermentação e respiração celular", OptionD = "Mitose e meiose", OptionE = "Transcrição e tradução",
                CorrectOption = 'B'
            },
            new() {
                Year = 2022, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Biologia",
                Topic = "Genética", Difficulty = "média",
                Statement = "O DNA contém a informação genética dos seres vivos. Qual é o processo pelo qual a informação do DNA é copiada para o RNA mensageiro?",
                OptionA = "Tradução", OptionB = "Replicação", OptionC = "Transcrição", OptionD = "Mitose", OptionE = "Recombinação",
                CorrectOption = 'C'
            },
            new() {
                Year = 2021, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Biologia",
                Topic = "Ecologia", Difficulty = "fácil",
                Statement = "Em uma cadeia alimentar, os organismos que produzem matéria orgânica a partir de energia solar são denominados:",
                OptionA = "Consumidores primários", OptionB = "Decompositores", OptionC = "Produtores", OptionD = "Consumidores secundários", OptionE = "Parasitas",
                CorrectOption = 'C'
            },
            new() {
                Year = 2020, Area = "Ciências da Natureza e suas Tecnologias", Subject = "Biologia",
                Topic = "Evolução", Difficulty = "média",
                Statement = "Segundo a Teoria Sintética da Evolução, a seleção natural age sobre a variabilidade genética de uma população. Qual é a principal fonte de variabilidade genética?",
                OptionA = "Epigenética", OptionB = "Mutações e recombinação gênica", OptionC = "Deriva genética exclusivamente", OptionD = "Adaptações fenotípicas", OptionE = "Migração de predadores",
                CorrectOption = 'B'
            },

            // ── Matemática e suas Tecnologias ────────────────────────────────
            new() {
                Year = 2023, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Topic = "Progressões", Difficulty = "fácil",
                Statement = "Uma progressão aritmética tem primeiro termo a₁ = 3 e razão r = 4. Qual é o décimo termo?",
                OptionA = "35", OptionB = "39", OptionC = "43", OptionD = "47", OptionE = "51",
                CorrectOption = 'B'
            },
            new() {
                Year = 2023, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Topic = "Álgebra básica", Difficulty = "fácil",
                Statement = "Qual é a solução da inequação 2x - 6 > 0?",
                OptionA = "x < 3", OptionB = "x > 3", OptionC = "x = 3", OptionD = "x < -3", OptionE = "x > -3",
                CorrectOption = 'B'
            },
            new() {
                Year = 2022, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Topic = "Funções", Difficulty = "média",
                Statement = "A função f(x) = x² - 4x + 3 tem raízes nos pontos:",
                OptionA = "x = 1 e x = 3", OptionB = "x = -1 e x = -3", OptionC = "x = 2 e x = 2",
                OptionD = "x = 0 e x = 4", OptionE = "x = 1 e x = -3",
                CorrectOption = 'A'
            },
            new() {
                Year = 2022, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Topic = "Geometria plana", Difficulty = "média",
                Statement = "Um terreno retangular tem 30 m de comprimento e 20 m de largura. Um muro é construído a 2 m da borda em todos os lados. Qual é o perímetro do muro?",
                OptionA = "100 m", OptionB = "108 m", OptionC = "116 m", OptionD = "124 m", OptionE = "132 m",
                CorrectOption = 'C'
            },
            new() {
                Year = 2021, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Topic = "Estatística descritiva", Difficulty = "média",
                Statement = "Em uma turma de 40 alunos, as notas foram: 10 alunos nota 6, 15 nota 7, 10 nota 8 e 5 nota 9. A média da turma é:",
                OptionA = "6,8", OptionB = "7,0", OptionC = "7,25", OptionD = "7,5", OptionE = "8,0",
                CorrectOption = 'C'
            },
            new() {
                Year = 2021, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Topic = "Funções exponenciais", Difficulty = "difícil",
                Statement = "Se 2^x = 32, qual é o valor de x?",
                OptionA = "3", OptionB = "4", OptionC = "5", OptionD = "6", OptionE = "8",
                CorrectOption = 'C'
            },
            new() {
                Year = 2020, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Topic = "Probabilidade", Difficulty = "média",
                Statement = "Uma urna contém 4 bolas vermelhas e 6 bolas azuis. Retirando-se uma bola ao acaso, a probabilidade de ser vermelha é:",
                OptionA = "1/4", OptionB = "2/5", OptionC = "3/5", OptionD = "1/2", OptionE = "4/6",
                CorrectOption = 'B'
            },
            new() {
                Year = 2020, Area = "Matemática e suas Tecnologias", Subject = "Matemática",
                Topic = "Trigonometria", Difficulty = "difícil",
                Statement = "Em um triângulo retângulo, o ângulo agudo α tem seno igual a 3/5. O valor do cosseno de α é:",
                OptionA = "3/4", OptionB = "4/5", OptionC = "4/3", OptionD = "5/3", OptionE = "5/4",
                CorrectOption = 'B'
            },

            // ── Linguagens, Códigos e suas Tecnologias ───────────────────────
            new() {
                Year = 2023, Area = "Linguagens, Códigos e suas Tecnologias", Subject = "Língua Portuguesa",
                Topic = "Ortografia", Difficulty = "fácil",
                Statement = "Assinale a alternativa em que todas as palavras estão corretamente acentuadas segundo o Acordo Ortográfico de 2009.",
                OptionA = "herói, júri, bônus", OptionB = "héroe, júri, bonus",
                OptionC = "herói, juri, bônus", OptionD = "heroi, júri, bônus", OptionE = "herói, júri, bonus",
                CorrectOption = 'A'
            },
            new() {
                Year = 2023, Area = "Linguagens, Códigos e suas Tecnologias", Subject = "Literatura",
                Topic = "Realismo", Difficulty = "média",
                Statement = "O Realismo brasileiro do século XIX é marcado pela análise psicológica dos personagens. Qual obra de Machado de Assis exemplifica melhor essa característica?",
                OptionA = "Iracema", OptionB = "Dom Casmurro", OptionC = "O Guarani", OptionD = "A Moreninha", OptionE = "Inocência",
                CorrectOption = 'B'
            },
            new() {
                Year = 2022, Area = "Linguagens, Códigos e suas Tecnologias", Subject = "Inglês",
                Topic = "Gramática inglesa", Difficulty = "fácil",
                Statement = "Choose the sentence in which the Present Perfect is used correctly:",
                OptionA = "I have went to the store yesterday", OptionB = "She has never been to Paris",
                OptionC = "They have saw that movie last week", OptionD = "He has go to school every day", OptionE = "We have ate lunch an hour ago",
                CorrectOption = 'B'
            },
            new() {
                Year = 2022, Area = "Linguagens, Códigos e suas Tecnologias", Subject = "Língua Portuguesa",
                Topic = "Interpretação de texto", Difficulty = "fácil",
                Statement = "No trecho 'A linguagem é um instrumento vivo que reflete o pensamento de um povo', a oração destacada exerce a função de:",
                OptionA = "Oração subordinada adjetiva restritiva", OptionB = "Oração subordinada adjetiva explicativa",
                OptionC = "Oração subordinada substantiva objetiva", OptionD = "Oração coordenada aditiva", OptionE = "Oração coordenada explicativa",
                CorrectOption = 'A'
            },
            new() {
                Year = 2021, Area = "Linguagens, Códigos e suas Tecnologias", Subject = "Inglês",
                Topic = "Conditional sentences", Difficulty = "média",
                Statement = "Read the sentence: 'If I had studied harder, I would have passed the exam.' This sentence expresses:",
                OptionA = "A real possibility in the future", OptionB = "A hypothetical situation in the past",
                OptionC = "A present habit", OptionD = "A general truth", OptionE = "A command",
                CorrectOption = 'B'
            },
            new() {
                Year = 2021, Area = "Linguagens, Códigos e suas Tecnologias", Subject = "Literatura",
                Topic = "Modernismo", Difficulty = "difícil",
                Statement = "O Modernismo brasileiro, iniciado com a Semana de Arte Moderna de 1922, caracterizou-se principalmente por:",
                OptionA = "Retorno aos cânones clássicos europeus", OptionB = "Ruptura com formas tradicionais e valorização da cultura nacional",
                OptionC = "Continuidade do Parnasianismo", OptionD = "Imitação fiel do Romantismo francês", OptionE = "Rejeição do verso livre",
                CorrectOption = 'B'
            },
            new() {
                Year = 2020, Area = "Linguagens, Códigos e suas Tecnologias", Subject = "Língua Portuguesa",
                Topic = "Figuras de linguagem", Difficulty = "média",
                Statement = "Identifique a figura de linguagem presente em: 'As estrelas são os olhos da noite.'",
                OptionA = "Metonímia", OptionB = "Hipérbole", OptionC = "Metáfora", OptionD = "Antítese", OptionE = "Ironia",
                CorrectOption = 'C'
            },

            // ── Ciências Humanas e suas Tecnologias ──────────────────────────
            new() {
                Year = 2023, Area = "Ciências Humanas e suas Tecnologias", Subject = "História",
                Topic = "Revolução Industrial", Difficulty = "média",
                Statement = "A Revolução Industrial iniciada na Inglaterra no século XVIII trouxe profundas transformações sociais. Qual das alternativas representa uma consequência direta desse processo?",
                OptionA = "Fortalecimento do sistema feudal", OptionB = "Crescimento do proletariado urbano",
                OptionC = "Declínio do capitalismo comercial", OptionD = "Redução da jornada de trabalho", OptionE = "Diminuição da produção agrícola",
                CorrectOption = 'B'
            },
            new() {
                Year = 2023, Area = "Ciências Humanas e suas Tecnologias", Subject = "Geografia",
                Topic = "Climatologia", Difficulty = "média",
                Statement = "O fenômeno El Niño provoca alterações climáticas em diversas regiões do mundo. No Brasil, qual região é mais afetada pela redução das chuvas durante esse fenômeno?",
                OptionA = "Amazônia", OptionB = "Cerrado", OptionC = "Nordeste", OptionD = "Sul", OptionE = "Pantanal",
                CorrectOption = 'C'
            },
            new() {
                Year = 2022, Area = "Ciências Humanas e suas Tecnologias", Subject = "Filosofia",
                Topic = "Filosofia Moderna", Difficulty = "média",
                Statement = "Para Immanuel Kant, o imperativo categórico pode ser resumido como:",
                OptionA = "Age de tal modo que trates a humanidade sempre como um fim e nunca como um meio",
                OptionB = "O fim justifica os meios", OptionC = "A virtude é o caminho do meio entre dois extremos",
                OptionD = "Só sei que nada sei", OptionE = "Penso, logo existo",
                CorrectOption = 'A'
            },
            new() {
                Year = 2022, Area = "Ciências Humanas e suas Tecnologias", Subject = "História",
                Topic = "Brasil República", Difficulty = "média",
                Statement = "O período conhecido como 'Era Vargas' (1930-1945) foi marcado por políticas de centralização do poder. Qual característica define o Estado Novo (1937-1945)?",
                OptionA = "Multipartidarismo e liberdade de imprensa", OptionB = "Governo ditatorial com suspensão da Constituição de 1934",
                OptionC = "Redemocratização e eleições diretas", OptionD = "Federalismo descentralizado", OptionE = "Modelo parlamentarista",
                CorrectOption = 'B'
            },
            new() {
                Year = 2021, Area = "Ciências Humanas e suas Tecnologias", Subject = "Sociologia",
                Topic = "Globalização", Difficulty = "fácil",
                Statement = "O processo de globalização intensificou os fluxos de capital, mercadorias e informações entre países. Um dos efeitos desse processo no Brasil foi:",
                OptionA = "Diminuição da dependência tecnológica", OptionB = "Aumento da participação em cadeias produtivas globais",
                OptionC = "Encerramento das exportações de commodities", OptionD = "Fechamento total da economia ao mercado externo", OptionE = "Redução das desigualdades regionais",
                CorrectOption = 'B'
            },
            new() {
                Year = 2020, Area = "Ciências Humanas e suas Tecnologias", Subject = "Filosofia",
                Topic = "Filosofia Antiga", Difficulty = "fácil",
                Statement = "Para Sócrates, a busca pelo conhecimento parte do reconhecimento da própria ignorância, expressa na máxima:",
                OptionA = "'Penso, logo existo'", OptionB = "'O homem é a medida de todas as coisas'",
                OptionC = "'Só sei que nada sei'", OptionD = "'O homem nasce livre'", OptionE = "'A virtude está no meio'",
                CorrectOption = 'C'
            },
            new() {
                Year = 2020, Area = "Ciências Humanas e suas Tecnologias", Subject = "Geografia",
                Topic = "Geopolítica", Difficulty = "média",
                Statement = "O BRICS é um grupo de países emergentes com grande influência na economia mundial. Quais países compõem originalmente esse grupo?",
                OptionA = "Brasil, Rússia, Índia, China e África do Sul", OptionB = "Brasil, Reino Unido, Índia, China e Austrália",
                OptionC = "Brasil, Rússia, Itália, China e África do Sul", OptionD = "Bolívia, Rússia, Índia, Chile e África do Sul", OptionE = "Brasil, Romênia, Indonésia, China e Argentina",
                CorrectOption = 'A'
            },
        };

        db.Questions.AddRange(questions);
        db.SaveChanges();
    }
}
