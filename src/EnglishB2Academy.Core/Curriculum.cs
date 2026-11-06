namespace EnglishB2Academy.Core;

public static class Curriculum
{
    public static IReadOnlyList<Lesson> Lessons { get; } = BuildLessons();
    public static readonly string[] Levels = ["A1", "A2", "B1", "B1+", "B2"];

    public static readonly ExamProfile Cambridge = new(
        "cambridge-b2-first", "Cambridge B2 First",
        "https://www.cambridgeenglish.org/exams-and-tests/qualifications/first/format/",
        "2026-11-06",
        [
            new("Reading and Use of English", 75, 7, 52, "Seven parts: multiple-choice cloze, open cloze, word formation, key-word transformations, multiple-choice reading, gapped text and multiple matching."),
            new("Writing", 80, 2, 2, "Two tasks, 140-190 words each. Part 1 is an essay; Part 2 is a choice of task."),
            new("Listening", 40, 4, 30, "Approximately 40 minutes. Recordings are heard twice. Four parts with different question types."),
            new("Speaking", 14, 4, 0, "Approximately 14 minutes per pair. Interview, individual long turn, collaborative task and discussion. Timing varies for groups.")
        ]);

    public static IReadOnlyList<VocabularyItem> Vocabulary { get; } =
    [
        new("appointment", "An arranged meeting at a particular time.", "I have a dental appointment on Thursday."),
        new("borrow", "Take something temporarily and return it.", "May I borrow your umbrella?"),
        new("lend", "Give something temporarily, expecting it back.", "I can lend you my spare bicycle."),
        new("journey", "The act of travelling from one place to another.", "Our journey took three hours."),
        new("commute", "Travel regularly between home and work or study.", "She commutes by train."),
        new("reliable", "Able to be trusted to work or behave well.", "The local bus service is reliable."),
        new("affordable", "Not too expensive for your budget.", "We need an affordable place to stay."),
        new("deadline", "The latest time by which something must be done.", "The application deadline is Friday."),
        new("achievement", "Something valuable that you succeed in doing.", "Finishing the course was an achievement."),
        new("drawback", "A disadvantage of a plan or situation.", "The main drawback is the long journey."),
        new("benefit", "An advantage or positive result.", "Daily walking has many health benefits."),
        new("evidence", "Information that supports a conclusion.", "We need evidence before making a decision."),
        new("sustainable", "Able to continue without exhausting resources.", "The town is investing in sustainable transport."),
        new("despite", "Used to contrast something with an unexpected result.", "Despite the rain, we enjoyed the walk."),
        new("whereas", "Used to compare two different facts.", "I prefer tea, whereas my brother likes coffee."),
        new("nevertheless", "Despite what has just been said.", "The task was difficult; nevertheless, we finished."),
        new("carry out", "Perform or complete a task.", "The team will carry out a survey."),
        new("put off", "Delay an activity until a later time.", "Do not put off revising until the last day."),
        new("look into", "Investigate a situation.", "The council will look into the complaint."),
        new("come across", "Find or meet something by chance.", "I came across an interesting article."),
        new("take into account", "Consider a factor when making a decision.", "Take travel costs into account."),
        new("make an effort", "Try hard to do something.", "He made an effort to learn everyone's name."),
        new("raise awareness", "Help more people know about an issue.", "The campaign raises awareness of food waste."),
        new("reach a conclusion", "Decide after considering information.", "We reached a conclusion after the discussion."),
        new("suit", "Be right for a person or situation.", "An evening class would suit my schedule."),
        new("fit", "Be the correct size or shape.", "These shoes fit perfectly."),
        new("economic", "Related to trade, industry or money.", "The report examines economic growth."),
        new("economical", "Using little money, energy or other resources.", "This small car is economical to run."),
        new("eventually", "At the end of a process or after a long time.", "We eventually found the address."),
        new("currently", "At the present time.", "The library is currently being repaired.")
    ];

    public static readonly string[] WritingPrompts =
    [
        "Essay: Your class has discussed ways to make a town more welcoming. Write about public spaces and local events, and add one idea of your own. Which change would help most? Write 140-190 words.",
        "Email: An English-speaking friend is moving to your town. Recommend a place to live, explain how to travel around and suggest one way to meet people. Write 140-190 words.",
        "Review: Write a review of a place where people can study. Describe its facilities, explain who would find it useful and suggest one improvement. Write 140-190 words.",
        "Article: A community magazine wants articles titled 'A skill worth learning'. Explain your choice, describe how to begin and encourage readers to try. Write 140-190 words."
    ];

    public static readonly string[] SpeakingPrompts =
    [
        "Interview: Talk about where you live, one activity you enjoy and something you would like to learn. Give reasons, not just short answers.",
        "Long turn (one minute): Compare studying alone at home with studying with a partner in a library. Why might each situation suit a different learner? This is a verbal comparison prompt, not an official photo task.",
        "Collaborative task (with a partner): A town has money for a garden, a cycle path, a youth club, a library room or a music event. Discuss the benefits, then agree on the best option.",
        "Discussion: How can local communities encourage lifelong learning? Should practical skills be taught in school? Develop your answers with examples."
    ];

    private static IReadOnlyList<Lesson> BuildLessons()
    {
        var lessons = new List<Lesson>();
        void Add(string id, string level, string skill, string title, string explanation,
            string example, string mistake, params string[] rows)
        {
            var questions = rows.Select((row, i) =>
            {
                var fields = row.Split('|');
                if (fields.Length != 6) throw new InvalidDataException($"Invalid authored question: {id}-{i}");
                return new Question($"{id}-{i + 1}", skill, level, fields[0],
                    [fields[1], fields[2], fields[3]], [fields[4]], fields[5]);
            }).ToArray();
            lessons.Add(new Lesson(id, level, skill, title, explanation, example, mistake, questions));
        }
        Add("a1-be", "A1", "Grammar", "Introducing yourself: be",
            "Use am with I, is with he/she/it, and are with you/we/they. In a question, put be before the subject. Use not to make a negative.",
            "I am a learner. Are they ready? She is not late.",
            "Say 'She is a teacher', not 'She a teacher': an English sentence normally needs a verb.",
            "My brother ___ a nurse.|am|is|are|is|Brother is singular, so use is.",
            "___ you ready to begin?|Is|Am|Are|Are|Use are with you; put it before the subject in a question.",
            "I ___ not from London.|are|am|is|am|Use am with I.");
        Add("a1-present", "A1", "Grammar", "Everyday routines",
            "Use the present simple for habits and facts. Add -s or -es for he/she/it. Use do/does for questions and do not/does not + base verb for negatives.",
            "She walks to work. Does she work on Fridays?",
            "After does, use the base verb: 'Does he play?', not 'Does he plays?'",
            "Mia ___ to work every day.|walk|walking|walks|walks|A singular third-person subject needs walks.",
            "Does your sister ___ here?|live|lives|living|live|Does is followed by the base form live.",
            "We ___ eat meat.|doesn't|don't|isn't|don't|Use do not with we.");
        Add("a1-articles", "A1", "Grammar", "Articles and things you can count",
            "Use a/an for one non-specific countable thing. Choose an before a vowel sound. Use the when the listener knows which thing. Uncountable nouns do not take a/an.",
            "An apple, a university, some water. Close the door beside you.",
            "'Advice' is uncountable: say 'some advice', not 'an advice'.",
            "I need ___ umbrella.|a|an|some|an|Umbrella begins with a vowel sound.",
            "Could I have ___ water?|a|an|some|some|Water is uncountable.",
            "Please close ___ door next to you.|a|an|the|the|The description identifies a particular door.");
        Add("a1-place", "A1", "Grammar", "Places and simple questions",
            "Use in for an enclosed area, on for a surface and at for a point. Use where for place, when for time and who for a person.",
            "The keys are on the table. Where is the station?",
            "A question needs the right word order: 'Where is it?', not 'Where it is?'",
            "The book is ___ the table's surface.|in|on|at|on|Use on for a surface.",
            "___ is your birthday: in May or June?|Who|Where|When|When|When asks about time.",
            "Choose the direct question.|Where it is?|Where is it?|Where it?|Where is it?|Put is before the subject in a direct question.");
        Add("a2-past", "A2", "Grammar", "Finished events in the past",
            "Use the past simple for finished events. Many verbs add -ed; some are irregular. Questions use did + base form. Negatives use did not + base form.",
            "We visited the museum yesterday. Did you go by bus?",
            "Use 'didn't go', not 'didn't went': did already carries past tense.",
            "We ___ the museum yesterday.|visit|visited|visiting|visited|Yesterday identifies a finished past time.",
            "Did you ___ the film?|saw|seen|see|see|After did, use the base form see.",
            "She ___ go out last night.|didn't|doesn't|isn't|didn't|Last night calls for a past negative.");
        Add("a2-continuous", "A2", "Grammar", "Actions happening around now",
            "The present continuous is am/is/are + -ing. Use it for an activity happening now or a temporary situation. Present simple describes routines.",
            "I usually cycle, but today I am taking the bus.",
            "Do not omit be: 'She is working', not 'She working'.",
            "Look! The dog ___ across the field.|runs every day|is running|run|is running|Look signals an action happening now.",
            "I usually ___ tea in the morning.|drink|am drinking|drinking|drink|Usually describes a routine.",
            "They ___ studying at the moment.|is|am|are|are|Use are with they.");
        Add("a2-future", "A2", "Grammar", "Plans and predictions",
            "Use going to for prior intentions and predictions based on visible evidence. Will often marks a decision made as you speak. The present continuous can describe an arranged future event.",
            "I have booked a ticket. I am travelling tomorrow.",
            "After will, use the base form, not to + verb.",
            "I've just decided: I ___ call her now.|will|am|was|will|Will marks a decision made now.",
            "We ___ meeting our tutor at six; it is arranged.|are|will|do|are|Present continuous describes a firm arrangement.",
            "Look at those dark clouds. It is going ___ rain.|for|to|at|to|The construction is going to + base verb.");
        Add("a2-comparison", "A2", "Grammar", "Comparing people and things",
            "Short adjectives often add -er/-est. Longer adjectives usually use more/most. Use than for comparison and the before a superlative. Good becomes better/best.",
            "This route is shorter than that one. It is the most convenient route.",
            "Do not double the comparative: 'easier', not 'more easier'.",
            "This exercise is ___ than the last one.|easy|easier|easiest|easier|Than requires a comparative.",
            "It is the ___ expensive option of all.|more|most|much|most|Of all signals a superlative.",
            "My second result was ___ than my first.|good|best|better|better|The comparative of good is better.");
        Add("b1-perfect", "B1", "Grammar", "Experiences and present results",
            "Use have/has + past participle for experience without a finished time, or for a past event connected to now. Use past simple with a stated finished time.",
            "I have visited York twice. I visited York last spring.",
            "Do not combine a finished time with this use of present perfect: 'I saw it yesterday', not 'I have seen it yesterday'.",
            "I ___ my keys, so I cannot open the door.|have lost|lose yesterday|losing|have lost|The lost keys have a present consequence.",
            "She ___ to York in 2022.|has been|went|has gone|went|A stated finished year takes past simple.",
            "Have you ever ___ a horse?|rode|ride|ridden|ridden|Have needs the past participle ridden.");
        Add("b1-modals", "B1", "Grammar", "Advice, permission and obligation",
            "Should gives advice. Must and have to express obligation. Must not means prohibition; do not have to means there is no obligation. Use the base verb after a modal.",
            "You should rest. You must not smoke here. You do not have to stay.",
            "Do not confuse 'mustn't' (not allowed) with 'don't have to' (not necessary).",
            "The sign says 'No entry'. You ___ go in.|don't have to|mustn't|should|mustn't|No entry expresses prohibition.",
            "Attendance is optional. You ___ come.|mustn't|don't have to|must|don't have to|Optional means there is no obligation.",
            "You look tired. You ___ rest.|should|ought|must to|should|Should + base verb gives advice.");
        Add("b1-conditionals", "B1", "Grammar", "Real and imagined possibilities",
            "First conditional: if + present simple, will + base verb for a real future possibility. Second conditional: if + past simple, would + base verb for an imagined situation.",
            "If it rains, we will stay inside. If I had more time, I would learn Italian.",
            "Normally do not use will in the if-clause of a first conditional.",
            "If it ___ tomorrow, we will stay home.|will rain|rains|rained|rains|The if-clause uses present simple for a future possibility.",
            "If I had a bicycle, I ___ cycle to work.|will|would|am|would|An imagined present situation uses would.",
            "If you heat ice, it ___.|melts|would melted|will melting|melts|A general truth can use present simple in both clauses.");
        Add("b1-relative", "B1", "Grammar", "Joining ideas with relative clauses",
            "Use who for people, which for things and where for places. Defining clauses identify the noun. Non-defining clauses add extra information and are separated by commas.",
            "The woman who lives next door is an artist.",
            "Do not repeat the subject: 'the man who lives here', not 'the man who he lives here'.",
            "The neighbour ___ helped us is a nurse.|which|who|where|who|Who refers to a person.",
            "This is the cafe ___ we first met.|where|who|whose|where|Where introduces information about a place.",
            "The bicycle ___ I bought is blue.|who|where|which|which|Which refers to a thing.");
        Add("b1plus-passive", "B1+", "Grammar", "Focusing on actions: the passive",
            "Form the passive with be in the appropriate tense + past participle. Use it when the action or its result matters more than the person doing it.",
            "The bridge was built in 1980. The road is being repaired.",
            "Keep the participle: 'was written', not 'was wrote'.",
            "The letters ___ delivered yesterday.|are|were|have|were|Yesterday and a plural subject require were.",
            "The report was ___ by the manager.|write|wrote|written|written|Passive be is followed by a past participle.",
            "The road is ___ repaired at the moment.|been|being|be|being|A current passive action uses is being + participle.");
        Add("b1plus-reported", "B1+", "Grammar", "Reporting what someone said",
            "When reporting past speech, we often move a tense back and adjust pronouns and time expressions. Reported questions use statement word order. Tell normally has an object; say does not need one.",
            "'I am tired,' she said. She said that she was tired.",
            "Say 'She told me', not 'She told that'.",
            "Yesterday she said she ___ busy at that moment.|is|was|will|was|In past reporting, am/is often moves back to was.",
            "Choose the reported question.|He asked where I lived.|He asked where did I live.|He asked where lived I.|He asked where I lived.|Reported questions use statement order.",
            "He ___ me that the meeting was cancelled.|said|told|spoke|told|Tell takes a person as its object.");
        Add("b1plus-gerunds", "B1+", "Grammar", "Gerunds and infinitives",
            "Some verbs take -ing, such as enjoy, avoid and suggest. Others take to + base verb, such as decide, hope and promise. After a preposition, normally use -ing.",
            "I enjoy reading. We decided to leave. Thank you for helping.",
            "Say 'I look forward to seeing you': to is a preposition in this expression.",
            "I enjoy ___ in the sea.|swim|swimming|to swim|swimming|Enjoy is followed by an -ing form.",
            "We decided ___ earlier.|to leave|leaving|leave|to leave|Decide takes a to-infinitive.",
            "Thank you for ___ me.|help|to help|helping|helping|Use -ing after the preposition for.");
        Add("b1plus-pastperfect", "B1+", "Grammar", "Before another past event",
            "Use had + past participle to show that one past event happened before another. Past continuous, was/were + -ing, gives background or an interrupted action.",
            "When I arrived, the train had left. I was reading when the phone rang.",
            "Use had left for the earlier event, not had leave.",
            "By the time I arrived, the train ___ left.|has|had|was|had|Had left marks an earlier past event.",
            "I ___ reading when the phone rang.|am|have|was|was|Past continuous gives the activity in progress.",
            "She had already ___ when I called.|eaten|ate|eat|eaten|Had requires the past participle eaten.");
        Add("b2-mixed", "B2", "Grammar", "Past regrets and present consequences",
            "Third conditional: if + had + participle, would have + participle. A mixed conditional connects an unreal past condition to a present result using would + base verb.",
            "If I had revised, I would have passed. If I had taken that job, I would live abroad now.",
            "Do not put would have in the standard if-clause: use if I had known.",
            "If I had known, I ___ have helped.|would|will|had|would|The unreal past result uses would have.",
            "If she had saved more, she ___ own a car now.|would|will|had|would|A present consequence of an unreal past condition uses would.",
            "If we ___ left earlier, we would have arrived on time.|have|had|would|had|The unreal past condition uses had + participle.");
        Add("b2-deduction", "B2", "Grammar", "Degrees of certainty",
            "Use must for a strong logical conclusion, might/could for possibility and cannot for something you believe impossible. For past deduction, use modal + have + participle.",
            "The lights are off; they might have gone home.",
            "A deduction is not a fact: choose language that matches the evidence.",
            "Her car is outside and I can hear her voice. She ___ be home.|must|can't|mustn't|must|The evidence strongly supports this conclusion.",
            "I am not sure why he is late. He ___ have missed the bus.|mustn't|might|can't|might|Might expresses an uncertain possibility.",
            "She was with us all evening. She ___ have been at the concert.|must|might|can't|can't|The evidence rules out the past event.");
        Add("b2-linking", "B2", "Grammar", "Contrast, purpose and complex sentences",
            "Although introduces a clause; despite is followed by a noun or -ing form. So that introduces a purpose clause. Whereas contrasts two facts.",
            "Although it rained, we walked. Despite the rain, we walked.",
            "Do not say 'despite it rained': use despite the rain or although it rained.",
            "___ the rain, the match continued.|Although|Despite|Even|Despite|Despite is followed by the noun phrase the rain.",
            "___ it was raining, the match continued.|Despite|Although|In spite|Although|Although introduces a full clause.",
            "I spoke slowly so ___ everyone could follow.|that|as|for|that|So that introduces a purpose clause.");
        Add("b2-inversion", "B2", "Grammar", "Emphasis and inversion",
            "After a fronted negative adverbial such as never or rarely, invert auxiliary and subject. A cleft sentence uses It is/was ... that to highlight information.",
            "Never have I seen such a view. It was Maya who found the answer.",
            "Keep the base verb after did: 'Rarely did he complain', not 'Rarely did he complained'.",
            "Never ___ I seen such a clear sky.|have|I have|had I have|have|Fronted never triggers auxiliary-subject inversion.",
            "Rarely did he ___ about the noise.|complained|complains|complain|complain|After did, use the base form.",
            "It was the delay ___ caused the problem.|what|that|where|that|A cleft sentence highlights the cause with that.");
        Add("a1-words", "A1", "Vocabulary", "Time and everyday activities",
            "Learn words in useful combinations. Have breakfast, catch a bus and make dinner are common everyday combinations.",
            "I have breakfast at seven, then catch the bus.",
            "Word-for-word translation can produce unnatural combinations. Notice which words normally go together.",
            "I ___ breakfast at seven.|do|have|make to|have|Have breakfast is a natural combination.",
            "A person who teaches is a ___.|teacher|kitchen|journey|teacher|A teacher helps people learn.",
            "The day after Monday is ___.|Sunday|Tuesday|Friday|Tuesday|Tuesday follows Monday.");
        Add("a2-words", "A2", "Vocabulary", "Travel and everyday requests",
            "Borrow means take temporarily; lend means give temporarily. A single journey is one trip from one place to another.",
            "Could I borrow your map? I can lend you mine.",
            "The person receiving something borrows it; the person providing it lends it.",
            "Could I ___ your pen until lunch?|borrow|lend|give|borrow|The speaker wants to receive it temporarily.",
            "I can ___ you my umbrella.|borrow|lend|take|lend|The speaker provides the umbrella temporarily.",
            "The train ___ took two hours.|journey|travelled|travels|journey|Journey is a countable noun for one trip.");
        Add("b1-words", "B1", "Vocabulary", "Study and work combinations",
            "A deadline is a latest acceptable time. Make progress and make an effort are useful collocations.",
            "We made progress by practising before the deadline.",
            "Say 'make progress', not 'do a progress': progress is normally uncountable.",
            "We need to ___ an effort.|do|make|get|make|Make an effort is the usual collocation.",
            "The ___ for applications is Friday.|deadline|drawback|benefit|deadline|A deadline is the final acceptable date.",
            "She has made excellent ___.|progress|a progress|progresses|progress|Progress is normally uncountable.");
        Add("b1plus-words", "B1+", "Vocabulary", "Phrasal verbs in context",
            "Carry out means perform, put off means postpone, and look into means investigate. Learn each expression in a sentence.",
            "The team carried out a survey and looked into the findings.",
            "Meaning comes from the whole expression; look into is not the same as simply look.",
            "The council will look ___ the complaint.|into|upward|out of|into|Look into means investigate.",
            "Do not put ___ revising until the last day.|up|off|over|off|Put off means postpone.",
            "The researchers carried ___ an experiment.|onward|away|out|out|Carry out means perform.");
        Add("b2-words", "B2", "Vocabulary", "Precision and academic combinations",
            "Economic refers to trade and money; economical means efficient with resources. Evidence supports an argument. Take into account means consider.",
            "The report provides evidence of economic change.",
            "Choose the precise word, not just a similar-looking one. An economical car is inexpensive to run.",
            "This car uses little fuel; it is ___.|economic|economical|economy|economical|Economical means efficient or inexpensive to run.",
            "We must take cost into ___.|account|place|effort|account|Take into account means consider.",
            "The campaign aims to ___ awareness.|rise|raise|grow up|raise|Raise awareness is the standard collocation.");

        void Passage(string id, string level, string skill, string title, string passage, params string[] questions)
        {
            Add(id, level, skill, title,
                skill == "Reading" ? "Read for the main idea first, then locate evidence for each answer. Do not select an answer merely because it repeats a word from the text." :
                "Listen once for the overall situation and again for detail. Notice corrections and contrasts. The audio is locally synthesised, not a human examination recording.",
                passage, "An answer must be supported by the text or recording, not just by general knowledge.", questions);
            var last = lessons[^1];
            lessons[^1] = last with { Questions = last.Questions.Select(q =>
                q with { Passage = skill == "Reading" ? passage : "", Audio = skill == "Listening" ? passage : "" }).ToArray() };
        }
        Passage("a1-reading", "A1", "Reading", "A library notice",
            "West Street Library opens at nine from Monday to Friday. On Saturday it opens at ten. It is closed on Sunday. You can borrow six books for three weeks. Please return books at the desk.",
            "When does the library open on Saturday?|Nine|Ten|Eleven|Ten|The notice gives ten as Saturday's opening time.",
            "How many books may you borrow?|Three|Five|Six|Six|The notice allows six books.",
            "Where should books be returned?|At the desk|At the station|At home|At the desk|The final sentence says to use the desk.");
        Passage("a2-reading", "A2", "Reading", "A change of plan",
            "Hi Sam, our walking group planned to meet at the station at eight, but repairs have closed the main road. Please meet at the park gate at half past eight instead. Bring lunch; the cafe will not open until next month. We will return before four. Jo",
            "Where should Sam meet the group?|The station|The park gate|The cafe|The park gate|Jo changes the meeting place to the park gate.",
            "Why should Sam bring lunch?|The cafe is not open yet|The walk ends at noon|Food is too expensive|The cafe is not open yet|The cafe opens next month, not today.",
            "What time is the new meeting?|Eight|Half past eight|Four|Half past eight|The message replaces eight with half past eight.");
        Passage("b1-reading", "B1", "Reading", "Repair rather than replace",
            "A monthly repair cafe has opened in Bellford. Volunteers help residents mend small household items. Visitors pay for any replacement parts, but advice and labour are free. Organiser Leila says the aim is not merely to reduce waste: people also learn skills from one another. The team cannot accept gas appliances because none of the volunteers is qualified to repair them.",
            "What might a visitor need to pay for?|Labour|Advice|Replacement parts|Replacement parts|Parts cost money; advice and labour do not.",
            "What additional aim does Leila mention?|Sharing skills|Selling new appliances|Training paid staff|Sharing skills|People learn skills from one another.",
            "Why are gas appliances excluded?|They are too large|Qualified expertise is unavailable|They are never repairable|Qualified expertise is unavailable|The volunteers lack the necessary qualification.");
        Passage("b1plus-reading", "B1+", "Reading", "A workplace experiment",
            "For six months, a design firm allowed employees to choose between starting at seven or at ten. Managers had expected communication problems, yet most teams reported fewer interruptions. Customer response times remained unchanged. However, new employees found it harder to ask spontaneous questions. The firm kept flexible starts but introduced a shared two-hour period each morning when everyone was available online.",
            "Which result surprised managers?|More interruptions|Fewer interruptions|Slower customer responses|Fewer interruptions|Managers feared problems, yet interruptions decreased.",
            "Who experienced a particular difficulty?|New employees|Customers|All managers|New employees|New starters struggled to ask spontaneous questions.",
            "Why was a shared period introduced?|To remove flexibility|To ensure people could contact colleagues|To reduce staff numbers|To ensure people could contact colleagues|The shared period restores access while keeping flexible starts.");
        Passage("b2-reading", "B2", "Reading", "When a trial becomes evidence",
            "A council celebrated a rise in cycling after opening a protected route. The headline figure was encouraging, but researcher Nia urged caution: the count had been taken during an unusually dry month, and no comparable route had been monitored. She did not oppose the scheme; indeed, interviews suggested that new riders felt safer. Her argument was that a promising intervention deserved stronger evaluation, not that it should be abandoned.",
            "What is Nia's main concern?|Cyclists dislike the route|The evidence is not sufficient to establish the effect|The route must close|The evidence is not sufficient to establish the effect|Weather and lack of comparison weaken the inference.",
            "Why are the interviews mentioned?|To show some positive evidence exists|To prove cycling numbers fell|To criticise every rider|To show some positive evidence exists|Feeling safer supports promise but does not settle the numerical claim.",
            "Which best describes Nia's attitude?|Supportive but cautious|Hostile and dismissive|Certain of complete success|Supportive but cautious|She supports better evaluation, not abandonment.");
        Passage("a1-listening", "A1", "Listening", "At the station",
            "The next train to Brookfield leaves at ten fifteen from platform three. A return ticket costs twelve pounds. Please keep your ticket for the return journey.",
            "Where is the train going?|Brookfield|West Street|Bellford|Brookfield|The announcement names Brookfield.",
            "Which platform is it?|Two|Three|Four|Three|The train leaves from platform three.",
            "How much is a return ticket?|Ten pounds|Twelve pounds|Fifteen pounds|Twelve pounds|The price is twelve pounds.");
        Passage("a2-listening", "A2", "Listening", "A telephone message",
            "Hello, this is the sports centre. Your swimming class on Tuesday has moved from six to seven in the evening. Please bring a towel and arrive ten minutes early. The class will be in the small pool, not the main pool.",
            "What time does the class now start?|Six|Seven|Eight|Seven|Seven replaces six.",
            "What should the learner bring?|A book|A towel|A ball|A towel|The message asks for a towel.",
            "Where will the class take place?|The main pool|The small pool|Outside|The small pool|The speaker corrects the expected location.");
        Passage("b1-listening", "B1", "Listening", "Choosing a course",
            "I considered the weekend photography course, but it required equipment I did not own. The evening course lets students borrow a camera. It lasts eight weeks rather than six, and the final session is an outdoor exhibition. That convinced me to choose it, even though the journey is longer.",
            "Why was the weekend course unsuitable?|Equipment was required|It was too long|It had no teacher|Equipment was required|The speaker did not own the necessary equipment.",
            "How long is the chosen course?|Six weeks|Eight weeks|Ten weeks|Eight weeks|The evening course lasts eight weeks.",
            "What particularly attracted the speaker?|A shorter journey|An outdoor exhibition|A cheaper bus ticket|An outdoor exhibition|The final exhibition convinced the speaker.");
        Passage("b1plus-listening", "B1+", "Listening", "A volunteer's reflection",
            "When I joined the river survey, I expected to spend most of my time collecting samples. In fact, careful record keeping took longer. At first that frustrated me, but I came to understand that a sample without a reliable date and location is almost useless. I still enjoy the fieldwork most, though I no longer regard the paperwork as a distraction.",
            "What took more time than expected?|Travelling|Record keeping|Buying tools|Record keeping|The speaker contrasts collecting with recording.",
            "How did the speaker's view change?|They valued the paperwork more|They stopped enjoying fieldwork|They abandoned the project|They valued the paperwork more|They learned why reliable records matter.",
            "What is still the speaker's favourite part?|Paperwork|Meetings|Fieldwork|Fieldwork|The speaker says they still enjoy fieldwork most.");
        Passage("b2-listening", "B2", "Listening", "Rethinking public consultation",
            "Our first survey attracted hundreds of responses, so we thought we had captured local opinion. Later, we noticed that almost all respondents already used the park. We then held short conversations at the bus station, where non-users explained that they found the entrances difficult to locate. The extra work did not overturn the survey; it revealed whose experience had been missing.",
            "What limitation was discovered in the first survey?|It mainly represented existing users|It had no responses|Its questions were unreadable|It mainly represented existing users|The respondents almost all used the park already.",
            "What problem did non-users identify?|Too many trees|Entrances were hard to find|Bus fares were rising|Entrances were hard to find|The new conversations revealed a navigation barrier.",
            "What did the extra consultation achieve?|It invalidated every earlier response|It added a neglected perspective|It proved no changes were needed|It added a neglected perspective|The speaker says it revealed missing experience.");
        Validate(lessons);
        return lessons;
    }

    public static void Validate(IReadOnlyList<Lesson> lessons)
    {
        var questions = lessons.SelectMany(l => l.Questions).ToArray();
        if (lessons.Select(l => l.Id).Distinct().Count() != lessons.Count ||
            questions.Select(q => q.Id).Distinct().Count() != questions.Length)
            throw new InvalidDataException("Duplicate content identifiers.");
        foreach (var question in questions)
            if (question.Answers.Length == 0 || question.Options.Length != question.Options.Distinct().Count() ||
                question.Answers.Any(a => !question.Options.Contains(a)) ||
                string.IsNullOrWhiteSpace(question.Explanation))
                throw new InvalidDataException($"Invalid answer key for {question.Id}.");
    }
}
