using UnityEngine;

// 鸡尾酒类（独立文件）
public class Cocktail
{
    public int strong;   // 浓烈度
    public int bitter;   // 苦度
    public int sour;     // 酸度
    public string[] steps; // 步骤记录

    public Cocktail()
    {
        strong = 0;
        bitter = 0;
        sour = 0;
        steps = new string[5]; // 5个步骤
    }

    // 添加物品属性
    public void AddItemAttributes(ItemData item)
    {
        strong += item.strongValue;
        bitter += item.bitterValue;
        sour += item.sourValue;

        // 确保属性非负
        strong = Mathf.Max(0, strong);
        bitter = Mathf.Max(0, bitter);
        sour = Mathf.Max(0, sour);
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