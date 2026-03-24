using UnityEngine;

// 鸡尾酒类（新增浓稠度）
public class Cocktail
{
    public int strong;   // 烈度
    public int bitter;   // 苦度
    public int thick;    // 浓稠度（新增）
    public string[] steps; // 步骤记录（8步）

    public Cocktail()
    {
        strong = 0;
        bitter = 0;
        thick = 0; // 初始化浓稠度
        steps = new string[8]; // 步骤数适配新流程
    }

    // 添加物品属性（新增浓稠度）
    public void AddItemAttributes(ItemData item)
    {
        strong += item.strongValue;
        bitter += item.bitterValue;
        thick += item.thickValue; // 累加浓稠度

        // 确保属性非负
        strong = Mathf.Max(0, strong);
        bitter = Mathf.Max(0, bitter);
        thick = Mathf.Max(0, thick);
    }

    // 记录步骤
    public void RecordStep(int stepIndex, string stepDesc)
    {
        if (stepIndex >= 0 && stepIndex < steps.Length)
        {
            steps[stepIndex] = stepDesc;
        }
    }
}