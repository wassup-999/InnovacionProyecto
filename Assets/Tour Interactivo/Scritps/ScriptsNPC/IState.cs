
//-> Definir el contrado que todo ESTADO debe cumplor
//-> al usar una interfaz podemos hacer que la clase trabej con cualquier script que Implemente esta interfaz
public interface IState
{
    //->se llama UNA VEZ cuando el estado se activa
    void Enter();

    //-> se llamada CADA FRAME mientras el estado esta activo
    void Update();

    //-> se lllama UNA VEZ cuando el estado esta apunto de cambiar
    void Exit();
}