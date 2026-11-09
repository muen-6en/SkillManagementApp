namespace SkillManagementApp.Data;

public sealed record SkillSuggestionQuestion(
    string Id,
    string Text,
    IReadOnlyList<string> Options);

public sealed record SkillSuggestionCandidate(
    string Name,
    string Capability,
    string Priority,
    string Reason);

public static class SkillSuggestionCatalog
{
    public const string WorkProblems = "work-problems";
    public const string FutureWork = "future-work";
    public const string CurrentSituation = "current-situation";

    public static IReadOnlyList<(string Id, string Label)> EntryPoints { get; } =
    [
        (WorkProblems, "いま仕事で何が困っているか"),
        (FutureWork, "将来やりたい仕事から考える"),
        (CurrentSituation, "今の自分の状況から考える"),
    ];

    public static IReadOnlyList<SkillSuggestionQuestion> Questions(string entryPoint) =>
        entryPoint switch
        {
            FutureWork =>
            [
                new("future-role", "将来、どのような仕事内容に近づきたいですか？", ["専門性を深めたい", "人やチームを支えたい", "仕組みを作りたい"]),
                new("future-environment", "どのような環境で働きたいですか？", ["安定して集中できる環境", "変化が多く挑戦できる環境", "協力して進める環境"]),
                new("future-strength", "その仕事で活かしたい強みは何ですか？", ["論理的に考える力", "人に伝える力", "手を動かして作る力"]),
                new("future-gap", "今との差を感じるのはどこですか？", ["知識", "実践経験", "説明・コミュニケーション"]),
                new("future-note", "将来やりたい仕事について、自由に補足してください。", []),
            ],
            CurrentSituation =>
            [
                new("current-inventory", "今のスキル状況に近いものはどれですか？", ["得意分野が分かっている", "経験はあるが整理できていない", "何から始めるか分からない"]),
                new("current-analysis", "自分の状況を確認するとき、何を使っていますか？", ["スキルマップ", "分析ツール", "まだ決まっていない"]),
                new("current-comparison", "参考にしたい相手や役割はありますか？", ["ある", "少しある", "特にない"]),
                new("current-action", "まず整理したいものは何ですか？", ["自分のタスク", "チームのタスク", "将来必要なスキル"]),
                new("current-note", "現在の状況について、自由に補足してください。", []),
            ],
            _ =>
            [
                new("work-time", "時間がかかっている作業はありますか？", ["ある", "少しある", "特にない"]),
                new("work-trouble", "面倒・苦手だと感じる作業はありますか？", ["ある", "少しある", "特にない"]),
                new("work-support", "人に頼った作業はありますか？", ["ある", "少しある", "特にない"]),
                new("work-team", "チームで困っていることはありますか？", ["ある", "少しある", "特にない"]),
                new("work-note", "困っている仕事内容を自由に補足してください。", []),
            ],
        };

    public static IReadOnlyList<SkillSuggestionCandidate> Candidates(string entryPoint) =>
        entryPoint switch
        {
            FutureWork =>
            [
                new("業務設計", "仕事を整理し、仕組みにする力", "高", "将来の仕事内容と現在との差を整理し、仕事全体を設計する力を候補にします。"),
                new("プログラミング", "仕組みを作り、自動化する力", "中", "仕組みを作りたい場合に、実装によって選択肢を広げられます。"),
                new("ドキュメント作成", "考えを整理して伝える力", "中", "専門知識や仕事の内容を他者へ伝える基礎になります。"),
            ],
            CurrentSituation =>
            [
                new("スキルマップ作成", "自分の経験と不足を可視化する力", "高", "現在のスキル状況を整理し、次に学ぶ対象を選びやすくします。"),
                new("タスク分析", "仕事を分解して課題を見つける力", "高", "自分やチームのタスクを棚卸しするための候補です。"),
                new("情報収集", "必要な知識や教材を探す力", "中", "何から始めるか分からない状態から、学習対象を具体化します。"),
            ],
            _ =>
            [
                new("業務効率化", "作業を見直し、時間を短縮する力", "高", "時間がかかる作業や面倒な作業の改善に結び付きます。"),
                new("自動化", "繰り返し作業を仕組みで減らす力", "高", "手作業を減らし、作業の余白を作る候補です。"),
                new("コミュニケーション", "依頼・相談・共有を円滑にする力", "中", "人に頼る作業やチームの困りごとの整理に役立ちます。"),
            ],
        };
}
