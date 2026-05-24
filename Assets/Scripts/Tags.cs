using UnityEngine;

public static class Tags
{
    public static readonly int Grounded = Animator.StringToHash("Grounded");
    public static readonly int Jump = Animator.StringToHash("Jump");
    public static readonly int AnimState = Animator.StringToHash("AnimState");
    public static readonly int AirSpeedY = Animator.StringToHash("AirSpeedY");
    public static readonly int WallSlide = Animator.StringToHash("WallSlide");
    public static readonly int Death = Animator.StringToHash("Death");
    public static readonly int Hurt = Animator.StringToHash("Hurt");
    public static readonly int Attack1 = Animator.StringToHash("Attack1");
    public static readonly int Attack2 = Animator.StringToHash("Attack2");
    public static readonly int Attack3 = Animator.StringToHash("Attack3");
    public static readonly int Block = Animator.StringToHash("Block");
    public static readonly int IdleBlock = Animator.StringToHash("IdleBlock");
    public static readonly int Roll = Animator.StringToHash("Roll");
    public static readonly int NoBlood = Animator.StringToHash("noBlood");
}