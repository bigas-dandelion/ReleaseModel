## Изменения версии 0.6.0 alpha от 01.04.2024 (не шутка)
*Изменения интерфейса:*
* Для LBM добавлен интерфейс, который позволяет задавать и менять в процессе расчетов параметр Tau и Gc (для модификации Шань-Ченя).
* Для LBM выведены настройки визуализации (предел, относительно которого считается градиент).
* Для LBM выведена в интерфейс возможность делать/не делать скриншоты и с какой частотой.

*Нововведения:*
* В сценариях к LBM добавлена возможность задавать начальную массу в клетках для каждого компонента.
* В примеры сценариев добавлены соответствующие изменения.

## Руководство по работе с программным пакетом Genstruct5 версии 0.5.0 alpha

### 1 Архитектура
Программный пакет Genstruct5 состоит из двух основных модулей и одного опционального - движка Гегель версии 2.0, моделей и интерфейса соответственно. Каждый из них находится в отдельном проекте.

Взаимодействие между моделями и интерфейсом осуществляется через паттерн Model-View-Presenter, суть которого заключается в том, что при каждом обновлении логики (Model) или интерфейса (View) запускается соответствующее событие. Presenter подписывается на события и при их срабатывании запускает соответствующие методы, например, при завершении итерации работы клеточного автомата он обновляет его отрисовку в интерфейсе. В свою очередь, данные, которыми должны обмениваться Model и View хранятся в классах ```ModelView``` и ```ViewModel```, которые будут рассмотрены ниже.

Движок Гегель 2.0 находится в проекте HegelEngine2 и содержит основные классы для создания клеточно-автоматной модели.

#### <u>Класс CellularAutomaton</u>
Главным классом в модуле является класс CellularAutomaton:
![2024-01-29_15-10-07](https://github.com/KayAltos9104/GenStruct5/assets/86559292/ae6da0af-8ccc-4223-a906-53d491248d44)

Он содержит два поля - ```_neighbors``` и ```_statesNumbers```. Первое поле содержит список _относительных_ координат клеток, являющихся соседними для любой клетки. Список хранит координаты в собственном типе ```VectorInt```. Словарь ```_statesNumbers``` содержит пары "название состояния - численное значение" для состояния клетки, чтобы можно было присваивать состояния клеткам через осмысленные названия.

Описание работы автомата содержится в двух основных методах: ```InitializeAutomata```, который готовит автомат к работе - создает поле, инициализирует значения параметров и так далее, и ```Update```, который содержит правила перехода, проверяет автомат на завершение работы, а также экспортирует выходные данные.

Метод ```InitializeAutomata``` является одинаковым для всех наследников и не может быть переопределен. Он создает пустое поле заданного размера, создает начальную конфигурацию автомата (начальные значения клеткам), генерирует координаты соседей в список ```_neighbors``` в зависимости от заданной окрестности, и запускает событие ```Initialized```, сообщающее системе о том, что автомат готов к работе:
```
public void InitializeAutomata()
{
    CreateEmptyField();
    CreateInitialConfiguration();
    switch (_inputParameters.Neighborhood)
    {
        case NeighborType.Neiman:
            {
                _neighbors = NeimanGenerate(_inputParameters.NeighborhoodOrder);
                break;
            }
        case NeighborType.Moore:
        default:
            {
                _neighbors = MooreGenerate(_inputParameters.NeighborhoodOrder);
                break;
            }
    }

    if (Is3D() == false)
        RemoveOZCoords(_neighbors);
    Status = AutomatonState.Ready;
    Initialized?.Invoke(this, EventArgs.Empty);
}
```
Метод ```CreateEmptyField``` может быть переопределен, хотя обычно в этом нет необходимости, так как клетки с состоянием "empty" (0) есть у всех автоматов:
```
public virtual void CreateEmptyField ()
{
    _outputParameters.FieldAG = new Cell[_inputParameters.Size.X, _inputParameters.Size.Y, _inputParameters.Size.Z];
    ProcessField(
        (x, y, z) =>
        _outputParameters.FieldAG[x, y, z] = new Cell(new VectorInt(x, y, z), _statesNumbers["empty"])
    );        
}
```
Метод ```CreateInitialConfiguration``` является абстрактным и должен быть определен у всех наследников класса, задавая начальное состояние всех клеток автомата.
Далее рассмотрим, как задавать параметры автомата.

#### <u>Входные и выходные параметры</u>
Входные и выходные параметры определяются не внутри клеточного автомата, а в отдельных классах - ```ViewModel``` и ```ModelView``` соответственно.

Класс ```ViewModel``` содержит входные параметры автомата, которые могут быть заданы через интерфейс пользователя, конфигурационный файл, вручную и т.д. Исходный класс-родитель содержит входные параметры, которые должен иметь каждый клеточный автомат: размер поля, порядок и тип окрестности, а также граничные условия:
```
public class ViewModel
{
    public (int X, int Y, int Z) Size { get; set; }
    public byte NeighborhoodOrder { get; set; }

    public CellularAutomaton.NeighborType Neighborhood { get; set; }
    public CellularAutomaton.BoundaryConditions BoundaryConditions { get; set; }
}
```
Аналогично для класса ```ModelView```, только он содержит те параметры, которые автомат передает во внешнюю среду: поле автомата, номер итерации, список измененных клеток (<u><b>в настоящий момент не используется</b></u>), номер итерации, текущее состояние автомата, время расчета:
```
public class ModelView
{
    public int Iteration { get; set; } = 0;
    public Cell[,,] FieldAG { get; set; }    
    public ConcurrentList<VectorInt> ChangedCells { get; set; } = new ConcurrentList<VectorInt>();   
    public bool IsFinished { get; set; } = false;
    public CellularAutomaton.AutomatonState State { get; set; }

    public TimeSpan ElapsedTime { get; set; }
    
    public override string ToString()
    {
        string outputInfo = "";
        outputInfo += $"Итерация : {Iteration}\n";
        outputInfo += $"Время на итерацию, мс : {string.Format("{0:f}", 1.0* ElapsedTime.TotalMilliseconds / Iteration )}\n";
        outputInfo += $"Расчет окончен : {IsFinished}\n";
        outputInfo += $"Время расчета, с : {string.Format("{0:f}", ElapsedTime.TotalSeconds)}\n";
        

        return outputInfo;
    }
}
```
Также здесь есть метод ToString, который описывает формат выводы параметров автомата.

*При создании нового клеточного автомата должны быть созданы также наследники классов ```ModelView``` и ```ViewModel``` для определения параметров для создаваемого автомата.*

#### <u>Поле клеток</u>
Выше в классе ```ModelView``` было приведено поле клеток автомата, которое содержит все клетки и их состояние. Поле хранится в виде трехмерного массива (в случае двухмерного автомата третье измерение просто равно 1) клеток, а сами клетки имеют свой собственный класс Cell:
```
public class Cell
{
    public string? LocalConfigurationId { get; private set; } = null;
    public VectorInt Pos { get; }
    public float State { get; set; }

    public Cell (VectorInt pos, float state)
    {
        Pos = pos;
        State = state;
    }

    public Cell(int x, int y, int z, float state)
    {
        Pos = new VectorInt(x,y,z);
        State = state;
    }

    public Cell (VectorInt pos, float state, string id) : this (pos, state)
    {
        ChangeLocalConfiguration(id);
    }
    public void ChangeLocalConfiguration (string id)
    {
        LocalConfigurationId = id;
    }    
}
```
Класс ```Cell``` содержит состояние клетки в формате ```float (State)``` и ее координату в формате ```VectorInt (Pos)```. Поле ```LocalConfigurationId``` содержит уникальный идентификатор локальной конфигурации, которой принадлежит клетка. Локальная конфигурация - это набор клеток, рассматриваемых как единое целое. Необходимость в ней возникает, например, для описания движения единых объектов, состоящих из нескольких клеток таких, как глобулы аэрогеля. В случае, если необходимости в ней нет, можно проставить принадлежность как ```null```. Подробнее локальные конфигурации будут рассмотрены далее. 

*В отличие от пакета GenStruct4 здесь класс клетки для всех автоматов одинаковый, поэтому в текущей версии нет необходимости создавать наследника для каждого типа клеток, что повышает удобство работы.*

### 2 Создание собственного клеточного автомата
Структура клеточного автомата в Genstruct5 выглядит следующим образом:
![2024-01-29_15-09-51](https://github.com/KayAltos9104/GenStruct5/assets/86559292/ed39866f-800b-4172-8194-581cebe93985)



Для создания нового клеточного автомата необходимо создать всего три класса-наследника от ```CellularAutomaton```, ```ModelView``` и ```ViewModel``` соответственно. Рассмотрим создание нового клеточного автомата на примере игры "Жизнь", которая уже есть в текущей версии.

Начнем с параметров автомата. По сравнению с классом-родителем в игру "Жизнь" добавляется только один новый параметр - процент живых клеток в начальной конфигурации:
```
public class GameOfLifeViewModel:ViewModel
{
    private float _lifeFraction;
    public float LifeFraction
    {
        get
        {
            return _lifeFraction;
        }
        set
        {
            if (value < 0)
                _lifeFraction = 0;
            else if (value > 1)
                _lifeFraction = 1;
            else
                _lifeFraction = value;
        }
    }

    public int Iterations { get; set; }
}
```
Определим его как дробное число от 0 до 1 и пропишем дополнительные условия на свойства, чтобы не допустить некорректных значений. Кроме того, добавим поле ```Iterations```, которое содержит количество шагов, через которое автомат завершит работу.
В выходные параметры допишем текущее количество живых клеток и соответствующим образом добавим вывод этого параметра через переопределение  метода ```ToString```:
```
public class GameOfLifeModelView:ModelView
{
    public int AliveNumber { get; set; }
    public override string ToString()
    {
        var output = base.ToString();
        output += $"Кол-во живых клеток: {AliveNumber}\n";
        return output;
    }
}
```
Теперь опишем клеточный автомат. Допишем в конструктор автомата второе возможное состояние клеток - живая:
```
public GameOfLifeModel(ModelView output, ViewModel input) : base(output, input)
{
    _statesNumbers.Add("alive", 1);
}
```
Далее опишем создание начальной конфигурации:
```
public override void CreateInitialConfiguration()
{
    (_outputParameters as GameOfLifeModelView).AliveNumber = 0;
    ProcessField((x, y, z) =>
    {
        _outputParameters.FieldAG[x, y, z].State = RollAlive((_inputParameters as GameOfLifeViewModel).LifeFraction)
    ? _statesNumbers["alive"]
    : _statesNumbers["empty"];
        if (GetCell(x, y, z).State == _statesNumbers["alive"]) (_outputParameters as GameOfLifeModelView).AliveNumber++;
        _outputParameters.ChangedCells.Add(new VectorInt(x,y,z));
    }
    );
}

private bool RollAlive (float probability)
{
    return Globals.Rnd.NextDouble() <= probability;
}
```
Данный метод записывает в состояние клетки 0 или 1 (empty или alive) с вероятностью ```LifeFraction```. Метод ```ProcessField``` принимает на вход делегат типа ```void (int x, int y, int z)```, который применяется ко всем клеткам массива поля. Такая запись полностью эквивалентна следующему коду:
```
for (int z = 0; z < _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Z); z++)
{
    for (int y = 0; y < _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Y); y++)
    {
        for (int x = 0; x < _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.X); x++)
        {
            _outputParameters.FieldAG[x, y, z].State = RollAlive((_inputParameters as GameOfLifeViewModel).LifeFraction)
? _statesNumbers["alive"]
: _statesNumbers["empty"];
            if (GetCell(x, y, z).State == _statesNumbers["alive"]) (_outputParameters as GameOfLifeModelView).AliveNumber++;
            _outputParameters.ChangedCells.Add(new VectorInt(x, y, z));
        }
    }
}
```

Подробнее смотрите класс ```CellularAutomaton```.

Следующим шагом является описание правил перехода автомата. Для начала напишем проверку автомата на завершение работы и создадим буферное поле, так как игра "Жизнь" является синхронным автоматом: 
```
if (_outputParameters.IsFinished)
{
    Status = AutomatonState.Finished;
    return;
}

int alive = 0;
Cell[,,] bufferField = new Cell[
    _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.X),
    _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Y),
    _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Z)];
```
Далее опишем правила перехода:
```
ProcessField((x, y, z) =>
{
    int liveNeighbors = 0;
    var checkPos = new VectorInt(0, 0, 0);
    foreach (var n in _neighbors)
    {
        checkPos.X = x + n.X;
        checkPos.Y = y + n.Y;
        checkPos.Z = z + n.Z;

        ProcessBorder(checkPos.X, checkPos.Y, checkPos.Z, out float state);
        if (state == _statesNumbers["alive"]) liveNeighbors++;
    }
    if (liveNeighbors == 3)
    {
        bufferField[x, y, z] = new Cell(x, y, z, _statesNumbers["alive"]);                
    }
    else if (liveNeighbors == 2)
    {
        bufferField[x, y, z] = new Cell(x, y, z, GetCell(x, y, z).State);               
    }
    else
    {
        bufferField[x, y, z] = new Cell(x, y, z, _statesNumbers["empty"]);
    }
});
```
Переменная ```checkPos``` содержит координату клетки-соседа, состояние которой в данный момент проверяется. Метод ```ProcessBorder``` обрабатывает те случаи, когда клетка выходит за границу поля в соответствии с заданными граничными условиями. Далее в соответствии с количеством живых соседей клетке буферного поля присваивается соответствующее состояние.
После этого осуществляется перенос состояний буферного поля в основное:
```
ProcessField((x, y, z) =>
{
    GetCell(x, y, z).State = bufferField[x, y, z].State;
    if (GetCell(x,y,z).State == _statesNumbers["alive"]) alive++;
}
);

(_outputParameters as GameOfLifeModelView).AliveNumber = alive;
```
В завершении необходимо вызвать метод ```Update``` родительского класса. Это необходимо делать всегда, так как там осуществляется проверка автомата на завершение и вызов события ```Updated```, которое сообщает системе о том, что итерация завершена. Целиком код метода ```Update``` выглядит следующим образом:
```
 public override void Update()
 {        
     if (_outputParameters.IsFinished)
     {
         Status = AutomatonState.Finished;
         return;
     }

     int alive = 0;
     Cell[,,] bufferField = new Cell[
         _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.X),
         _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Y),
         _outputParameters.FieldAG.GetLength((int)VectorInt.Dimension.Z)];

     ProcessField((x, y, z) =>
     {
         int liveNeighbors = 0;
         var checkPos = new VectorInt(0, 0, 0);
         foreach (var n in _neighbors)
         {
             checkPos.X = x + n.X;
             checkPos.Y = y + n.Y;
             checkPos.Z = z + n.Z;

             ProcessBorder(checkPos.X, checkPos.Y, checkPos.Z, out float state);
             if (state == _statesNumbers["alive"]) liveNeighbors++;
         }
         if (liveNeighbors == 3)
         {
             bufferField[x, y, z] = new Cell(x, y, z, _statesNumbers["alive"]);                
         }
         else if (liveNeighbors == 2)
         {
             bufferField[x, y, z] = new Cell(x, y, z, GetCell(x, y, z).State);               
         }
         else
         {
             bufferField[x, y, z] = new Cell(x, y, z, _statesNumbers["empty"]);
         }
     });
     ProcessField((x, y, z) =>
     {
         GetCell(x, y, z).State = bufferField[x, y, z].State;
         if (GetCell(x,y,z).State == _statesNumbers["alive"]) alive++;
     }
     );

     (_outputParameters as GameOfLifeModelView).AliveNumber = alive;
     base.Update();
     // А то картинка криво отображается из-за рассинхрона
     Thread.Sleep(17);
 }
```
Финальным шагом является определение метода ```IsFinished```, который проверяет автомат на завершение работы:
```
public override bool IsFinished()
{
    return _outputParameters.Iteration >= (_inputParameters as GameOfLifeViewModel).Iterations;
}
```

Логическая часть автомата готова. Далее рассмотрим интерфейс и визуализацию клеточного автомата.

### 3 Интерфейс работы с клеточным автоматом
А на этом месте мне стало лень, поэтому потом допишу.
