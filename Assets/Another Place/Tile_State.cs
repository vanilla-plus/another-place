using System;
using System.Threading.Tasks;

using UnityEngine;

[Serializable]
public class Tile_State
{
    [SerializeField]
    private bool _active;
    public bool active
    {
        get => _active;
        set
        {
            if (_active == value) return;

            _active = value;

            if (_active)
            {
                onActive?.Invoke();

                Fill();
            }
            else
            {
                onInactive?.Invoke();

                Drain();
            }
        }
    }

    public Action onActive;
    public Action onInactive;

    [SerializeField]
    public float _normal;
    public float normal
    {
        get => _normal;
        set => _normal = Mathf.Clamp01(value);
    }

    [SerializeField]
    public float normalRate = 2.0f;

    public Action onActiveNormalStart;
    public Action<float> onActiveNormalFrame;
    public Action onActiveNormalComplete;

    public Action onInactiveNormalStart;
    public Action<float> onInactiveNormalFrame;
    public Action onInactiveNormalComplete;

		

    private async Task Fill()
    {
        if (Mathf.Approximately(a: _normal,
            b: 0.0f)) onActiveNormalStart?.Invoke();

        while (_active && _normal < 1.0f)
        {
            normal += Time.deltaTime * normalRate;

            onActiveNormalFrame?.Invoke(_normal);

            await Task.Yield();
        }

        if (Mathf.Approximately(a: _normal,
            b: 1.0f)) onActiveNormalComplete?.Invoke();
    }


    private async Task Drain()
    {
        if (Mathf.Approximately(a: _normal,
            b: 1.0f)) onInactiveNormalStart?.Invoke();

        while (!_active &&
               _normal > 0.0f)
        {
            normal -= Time.deltaTime * normalRate;

            onInactiveNormalFrame?.Invoke(_normal);

            await Task.Yield();
        }

        if (Mathf.Approximately(a: _normal,
            b: 0.0f)) onInactiveNormalComplete?.Invoke();
    }

}
