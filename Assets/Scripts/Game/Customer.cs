using UnityEngine;

// 顾客类（固定顾客 + 固定需求）
public class Customer
{
    public string name;          // 顾客姓名
    public Sprite avatar;        // 顾客头像
    public int needStrong;       // 需求烈度
    public int needBitter;       // 需求苦度
    public int needThick;        // 需求浓稠度

    // 固定顾客（如需改需求，直接改这里）
    public Customer()
    {
        name = "Alex";
        needStrong = 10;
        needBitter = 8;
        needThick = 12;
        avatar = null;
    }
}