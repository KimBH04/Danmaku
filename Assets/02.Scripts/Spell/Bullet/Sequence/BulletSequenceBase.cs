using UnityEngine;

/// <summary>
/// 탄환에 대한 시퀀스를 데이터로 저장합니다.
/// </summary>
public abstract class BulletSequenceBase : ScriptableObject
{
    /// <summary>
    /// 시퀀스 실행자를 반환합니다.
    /// </summary>
    /// <returns>실행자</returns>
    public abstract ISequenceRunner GetSequenceRunner();

    /// <summary>
    /// 시퀀스 실행자 추상클래스
    /// </summary>
    public interface ISequenceRunner
    {
        /// <summary>
        /// 시퀀스가 시작된 직후 실행합니다.
        /// </summary>
        /// <param name="data">적용할 탄환 데이터</param>
        void Start(Bullet data);

        /// <summary>
        /// 탄환의 다음 움직임을 계산합니다.
        /// </summary>
        /// <param name="data">적용할 탄환 데이터</param>
        void Next(Bullet data);
    }
}
