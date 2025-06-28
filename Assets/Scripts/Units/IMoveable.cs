using UnityEngine;

public interface IMoveable
{
    void MoveTo(Vector3 position);
    void Stop();
}