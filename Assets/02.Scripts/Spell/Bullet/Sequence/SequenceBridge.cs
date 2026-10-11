using System.Linq;
using UnityEngine;

/// <summary>
/// 시퀀스와 시퀀스를 프레임 간격으로 이어줍니다.
/// </summary>
[CreateAssetMenu(fileName = "NewSequensBridge", menuName = "Bullet/Sequence Bridge", order = 2)]
public class SequenceBridge : BulletSequenceBase
{
    [SerializeField] private BulletSequenceBase[] sequences;

    [SerializeField] private int[] frameBetweenSequence;

    public override ISequenceRunner GetSequenceRunner()
    {
        return new SequenceBridgeRunner()
        {
            sequenceRunners = sequences.Select(s => s.GetSequenceRunner()).ToArray(),
            frameBetweenSequence = frameBetweenSequence,
        };
    }

    private class SequenceBridgeRunner : ISequenceRunner
    {
        public ISequenceRunner[] sequenceRunners;
        public int[] frameBetweenSequence;

        private int elapse = 0;
        private int index = 0;

        public void Start(Bullet data) { }

        public void Next(Bullet data)
        {
            sequenceRunners[index].Next(data);

            while (index < frameBetweenSequence.Length &&
                   index < sequenceRunners.Length - 1 &&
                   elapse >= frameBetweenSequence[index])
            {
                elapse = 0;
                index++;
                sequenceRunners[index].Next(data);
            }

            elapse++;
        }
    }
}