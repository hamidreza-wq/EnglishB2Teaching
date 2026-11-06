namespace EnglishB2Academy.Core;

public static class PracticeFactory
{
    public static PracticeSession Lesson(Lesson lesson) => Create(
        lesson.Title, lesson.Id, "Lesson", lesson.Questions, null, DateTimeOffset.UtcNow);

    public static PracticeSession Diagnostic(DateTimeOffset now) => Create(
        "Starting-point diagnostic", "diagnostic", "Diagnostic",
        Curriculum.Levels.SelectMany(level => Curriculum.Lessons
            .Where(l => l.Level == level && l.Skill is "Grammar" or "Vocabulary" or "Reading")
            .Take(3).Select(l => l.Questions[0])), 20, now);

    public static PracticeSession Exam(string kind, DateTimeOffset now)
    {
        var questions = ExamQuestions();
        return kind switch
        {
            "Reading" => Create("Timed reading and language practice", "exam-reading", "Exam",
                questions.Where(q => q.Skill != "Listening"), 30, now),
            "Listening" => Create("Timed listening practice", "exam-listening", "Exam",
                questions.Where(q => q.Skill == "Listening"), 15, now),
            "Mock" => Create("Short original practice mock", "exam-mock", "Exam", questions, 45, now),
            _ => throw new ArgumentException("Unknown practice paper.", nameof(kind))
        };
    }

    public static PracticeSession Revision(IEnumerable<Question> questions, DateTimeOffset now) =>
        Create("Mistake revision", "revision", "Revision", questions.DistinctBy(q => q.Id).Take(20), null, now);

    private static PracticeSession Create(string title, string source, string mode,
        IEnumerable<Question> questions, int? minutes, DateTimeOffset now)
    {
        var items = questions.ToList();
        if (items.Count == 0) throw new ArgumentException("A session needs questions.", nameof(questions));
        return new PracticeSession
        {
            Title = title, SourceId = source, Mode = mode, Questions = items,
            Started = now, Deadline = minutes is { } time ? now.AddMinutes(time) : null
        };
    }

    public static IReadOnlyList<Question> ExamQuestions()
    {
        var result = new List<Question>();
        void Add(string id, string skill, string prompt, string[] options, string answer, string explanation,
            string passage = "", string audio = "")
            => result.Add(new Question(id, skill, "B2", prompt, options, [answer], explanation, passage, audio));
        Add("exam-cloze-1", "Vocabulary", "The project made a significant ___ to local life.", ["contribution", "contribute", "contributing", "contributed"], "contribution", "A noun follows a significant.");
        Add("exam-cloze-2", "Vocabulary", "We need to ___ a decision before Friday.", ["do", "make", "get", "put"], "make", "Make a decision is the usual collocation.");
        Add("exam-cloze-3", "Vocabulary", "The committee took every suggestion into ___.", ["account", "reason", "thought", "purpose"], "account", "Take into account means consider.");
        Add("exam-cloze-4", "Vocabulary", "The long commute is the main ___ of the job.", ["drawback", "benefit", "achievement", "purpose"], "drawback", "A drawback is a disadvantage.");
        Add("exam-open-1", "Grammar", "Open cloze: She is interested ___ studying architecture. Write ONE word.", [], "in", "Interested is normally followed by in.");
        Add("exam-open-2", "Grammar", "Open cloze: Hardly had we arrived ___ the lights went out. Write ONE word.", [], "when", "The standard pattern is hardly ... when.");
        Add("exam-open-3", "Grammar", "Open cloze: You need not have bought milk; we already had ___. Write ONE word.", [], "some", "Some can stand for an unspecified quantity of milk here.");
        Add("exam-open-4", "Grammar", "Open cloze: This is the village in ___ my grandparents lived. Write ONE word.", [], "which", "After the preposition in, use which for a place.");
        Add("exam-formation-1", "Vocabulary", "Word formation: The instructions were clear and ___. HELP", [], "helpful", "Helpful is the adjective describing instructions.");
        Add("exam-formation-2", "Vocabulary", "Word formation: Her ___ improved after regular practice. CONFIDENT", [], "confidence", "The subject requires the noun confidence.");
        Add("exam-formation-3", "Vocabulary", "Word formation: The machine works ___ even in cold weather. RELIABLE", [], "reliably", "An adverb modifies works.");
        Add("exam-formation-4", "Vocabulary", "Word formation: The committee reached an ___ after two hours. AGREE", [], "agreement", "Agreement is the noun that follows an.");
        Add("exam-transform-1", "Grammar", "Key word transformation, DUE (2-5 words): They cancelled the walk because it rained. The walk was cancelled ___ the rain.", [], "due to", "Due to is followed by a noun phrase to give a cause.");
        Add("exam-transform-2", "Grammar", "Key word transformation, WISH (2-5 words): I regret not learning to swim. I ___ to swim.", [], "wish I had learned", "Wish + past perfect expresses a regret about the past.");
        result[^1] = result[^1] with { Answers = ["wish I had learned", "wish I had learnt"] };
        Add("exam-transform-3", "Grammar", "Key word transformation, BEEN (2-5 words): Someone has repaired the gate. The gate ___ repaired.", [], "has been", "Present perfect passive is has been + participle.");
        Add("exam-transform-4", "Grammar", "Key word transformation, UNLESS (2-5 words): You cannot enter if you do not have a ticket. You cannot enter ___ a ticket.", [], "unless you have", "Unless means if not.");

        const string reading = "When her town's cinema closed, Farah proposed turning its foyer into a shared workshop. " +
            "The council was initially hesitant, not because it opposed the idea, but because heating the entire building would be expensive. " +
            "Farah revised the plan so that only the foyer would be heated. A six-week trial attracted residents who had never joined local clubs. " +
            "Although the attendance figures pleased her, she considered the informal exchange of skills more important. " +
            "She resisted a sponsor's suggestion to charge for every visit, arguing that it might exclude precisely the people the project hoped to reach. " +
            "The trial did not prove the workshop could finance itself, so the council agreed to support a further evaluation rather than permanent funding.";
        Add("exam-read-1", "Reading", "Why was the council initially hesitant?", ["It disliked workshops", "It feared high running costs", "It wanted a new cinema", "It had no space"], "It feared high running costs", "Heating the building was the concern.", reading);
        Add("exam-read-2", "Reading", "What did Farah change?", ["The area to be heated", "The town's location", "The cinema's films", "The length of every visit"], "The area to be heated", "She restricted heating to the foyer.", reading);
        Add("exam-read-3", "Reading", "What mattered most to Farah?", ["Ticket sales", "Media coverage", "People sharing skills", "The sponsor's logo"], "People sharing skills", "She valued the informal exchange more than attendance figures.", reading);
        Add("exam-read-4", "Reading", "Why did she resist charging for every visit?", ["It could deter intended participants", "It was legally impossible", "The sponsor would leave", "Residents disliked the building"], "It could deter intended participants", "Charging might exclude people the project aimed to reach.", reading);
        Add("exam-read-5", "Reading", "What conclusion does the final sentence support?", ["Permanent funding is guaranteed", "More evidence is needed", "The trial failed completely", "The workshop is profitable"], "More evidence is needed", "The council funded further evaluation rather than a permanent commitment.", reading);
        Add("exam-read-6", "Reading", "Which best describes Farah's approach?", ["Flexible about methods, consistent about purpose", "Unwilling to change anything", "Interested only in money", "Certain no evaluation was needed"], "Flexible about methods, consistent about purpose", "She revised practical details while protecting access and learning.", reading);

        const string audio1 = "I almost chose the mountain volunteering week because the photographs looked impressive. " +
            "Then I read the schedule and realised that most tasks involved maintaining paths rather than observing wildlife. " +
            "There is nothing wrong with that, but it was not what I wanted. I joined the coastal survey instead. " +
            "We had to get up earlier, yet learning how to record seabird behaviour made the early starts worthwhile.";
        Add("exam-listen-1", "Listening", "Why did the speaker reject the mountain week?", ["The work did not match their interests", "It cost too much", "The photographs were unclear"], "The work did not match their interests", "The speaker wanted wildlife observation, not path maintenance.", audio: audio1);
        Add("exam-listen-2", "Listening", "What drawback did the chosen project have?", ["Early starts", "No instruction", "Expensive cameras"], "Early starts", "The coastal survey required getting up earlier.", audio: audio1);
        Add("exam-listen-3", "Listening", "How does the speaker feel about the chosen project?", ["It justified the inconvenience", "It was a complete mistake", "It should end immediately"], "It justified the inconvenience", "Learning made the early starts worthwhile.", audio: audio1);
        const string audio2 = "Our shop introduced a repair service last spring. We expected customers to use it mainly for expensive items. " +
            "Surprisingly, many brought inexpensive things with personal value. At first we offered repairs only on Saturdays, " +
            "but demand led us to add a Wednesday evening session. The service does not yet make a profit; " +
            "however, it has encouraged customers to visit us again, which was an unexpected benefit.";
        Add("exam-listen-4", "Listening", "What surprised the shop?", ["The personal value of items brought in", "The lack of customers", "The number of Saturday closures"], "The personal value of items brought in", "Many inexpensive items mattered personally to their owners.", audio: audio2);
        Add("exam-listen-5", "Listening", "When was an extra session introduced?", ["Monday morning", "Wednesday evening", "Sunday afternoon"], "Wednesday evening", "Wednesday evening was added to Saturday.", audio: audio2);
        Add("exam-listen-6", "Listening", "What unexpected benefit is mentioned?", ["More repeat visits", "Immediate profit", "Lower rent"], "More repeat visits", "Customers returned to the shop more often.", audio: audio2);
        const string audio3 = "The college planned to replace every printed handout with a digital version. " +
            "Students welcomed the searchable files, but several pointed out that they could not always charge their devices during the day. " +
            "Rather than abandon the plan, the college installed charging points and kept a small supply of paper copies. " +
            "The revised approach addressed access without losing the advantages of digital material.";
        Add("exam-listen-7", "Listening", "What practical concern did students raise?", ["Access to charging", "Too many paper copies", "Unsearchable files"], "Access to charging", "Their devices could run out of charge during the day.", audio: audio3);
        Add("exam-listen-8", "Listening", "What characterises the revised approach?", ["A compromise addressing access", "A total rejection of digital files", "A ban on paper"], "A compromise addressing access", "Charging points and some paper copies preserved both access and digital advantages.", audio: audio3);
        return result;
    }
}
