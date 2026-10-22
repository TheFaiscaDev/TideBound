using Godot;

/// <summary>
/// Base común para los estados de enemigos. Centraliza el acceso al enemigo
/// controlado y la gestión del Timer hijo (patrón repetido en todos los estados).
/// Convención de escena: cada estado tiene un hijo Timer llamado "Timer" cuya
/// señal timeout está conectada a OnTimerTimeout del estado.
/// </summary>
public abstract partial class EnemyState : State
{
    // El enemigo se obtiene del ControlledNode de la FSM (sin asignar en el inspector)
    protected Enemy Enemy => GetControlledNode<Enemy>();

    /// Inicia el Timer del estado. Si no existe, registra el error y desactiva
    /// la FSM para evitar soft-locks silenciosos (TransitionTo al estado actual no hace nada).
    protected bool StartTimerOrDisableFsm()
    {
        Timer timer = GetNodeOrNull<Timer>("Timer");
        if (timer == null)
        {
            GD.PushError($"Enemy '{Enemy?.Name}': el estado '{Name}' requiere un nodo Timer hijo llamado 'Timer'. Se desactiva la FSM.");
            if (fsm != null)
            {
                fsm.ProcessMode = ProcessModeEnum.Disabled;
            }
            return false;
        }
        timer.Start();
        return true;
    }

    /// Para el Timer para que no dispare transiciones fuera del estado.
    protected void StopTimer() => GetNodeOrNull<Timer>("Timer")?.Stop();
}
