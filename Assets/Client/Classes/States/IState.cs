using System.Collections;

namespace Client.Classes.States
{
    public interface IState
    {
        void Enter();
        IEnumerator Execute();
        void Exit();
    }
}
