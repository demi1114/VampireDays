/// スキルによって生成されるオブジェクトが実装するインターフェース
public interface ISkillObject
{
    /// 生成時の初期化
    void Initialize(RuntimeSkill runtimeSkill);
}