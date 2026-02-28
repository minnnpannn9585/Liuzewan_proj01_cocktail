using UnityEngine;

// 顾客类（独立文件）
public class Customer
{
    public string name;          // 顾客姓名
    public Sprite avatar;        // 顾客头像
    public int needStrong;       // 需求浓烈度
    public int needBitter;       // 需求苦度
    public int needSour;         // 需求酸度

    // 随机生成顾客
    public void GenerateRandomCustomer()
    {
        string[] names = { "张三", "李四", "王五", "赵六", "钱七" };
        name = names[Random.Range(0, names.Length)];
        
        needStrong = Random.Range(5, 16);
        needBitter = Random.Range(5, 16);
        needSour = Random.Range(5, 16);
    }
}