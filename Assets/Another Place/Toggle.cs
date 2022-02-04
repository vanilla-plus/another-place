
public class Toggle 
{
    private bool _active;
    public bool active 
    {
        get => _active;
        set 
        {
            if (_active == value) return;

            _active = value;

            onChange?.Invoke(_active);
        }
    }

    public Action<bool> onChange;

}

public class Normal 
{

    public Toggle active = new Toggle();
    public Toggle empty = new Toggle();
    public Toggle full = new Toggle();

    [SerializeField]
    private float _value;
    public float Value
    {
        get => _value;
        set => _value = Mathf.Clamp01(value);
    }

    [SerializeField]
    public float fillRate = 2.0f;

    [SerializeField]
    public float drainRate = 2.0f;

    public Action onFillStart;
    public Action<float> onFillFrame;
    public Action onFillComplete;

    public Action onDrainStart;
    public Action<float> onDrainFrame;
    public Action onDrainComplete;

    public Normal() 
    {
        active.onChange += active = 
        {
            if (active) 
            {
                Fill();
            }
            else 
            {
                Drain();
            }
        };
    }

    private async Task Fill()
    {
        if (_value < Mathf.Epsilon) 
        {
            empty.active = false;

            onFillStart?.Invoke();
        }

        while (active.active && _value < 1.0f)
        {
            Value += Time.deltaTime * fillRate;

            onFillFrame?.Invoke(_value);

            await Task.Yield();
        }

        if (1.0f - _value < Mathf.Epsilon) 
        {
            full.active = true;

            onFillComplete?.Invoke();
        }
    }


    private async Task Drain()
    {
        if (1.0f - _value < Mathf.Epsilon) 
        {
            full.active = false;

            onDrainStart?.Invoke();
        }

        while (!active.active && _value > 0.0f)
        {
            Value -= Time.deltaTime * drainRate;

            onDrainFrame?.Invoke(_value);

            await Task.Yield();
        }

        if (_value < Mathf.Epsilon) 
        {
            empty.active = true;

            onDrainComplete?.Invoke();
        }
    }
}

public class MenuBase<M, I> 
    where M : MenuBase<M, I>
    where I : MenuItemBase<M, I>
{

}

public class MenuItemBase<M,I>
    where M : MenuBase<M, I>
    where I : MenuItemBase<M, I>
{
    
    private static I _current;
    public static I Current 
    {
        get => _current;
        set 
        {
            if (ReferenceEquals(_current, value)) return;

            var old = _current;

            _current = value;

            onNewSelection?.Invoke(old, _current));
        }
    }

    public static Action<I,I> onNewSelection;

    public Toggle preview;
    public Toggle selected;

}