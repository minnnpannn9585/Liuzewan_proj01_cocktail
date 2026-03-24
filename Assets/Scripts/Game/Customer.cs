using UnityEngine;

// 顾客类（新增浓稠度需求）
public class Customer
{
    public string name;          // 顾客姓名
    public Sprite avatar;        // 顾客头像
    public int needStrong;       // 需求烈度
    public int needBitter;       // 需求苦度
    public int needThick;        // 需求浓稠度（新增）

    // 随机生成顾客
    public void GenerateRandomCustomer()
    {
        string[] names = { "Alex", "Blake", "Charlie", "Diana", "Ethan"  };
        name = names[Random.Range(0, names.Length)];
        
        // 需求范围5-15
        needStrong = Random.Range(5, 16);
        needBitter = Random.Range(5, 16);
        needThick = Random.Range(5, 16); // 新增浓稠度需求
    }
}