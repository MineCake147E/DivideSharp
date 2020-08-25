![DivideSharp Logo](DivideSharp-Logo.svg)

# DivideSharp - .NET Standard Integer Division Library

Like RyuJIT does before running, the DivideSharp optimizes an integer division by "mostly constant" values.

## Currently supported features

- Dividing two `uint` values quickly using `UInt32Divisor`
  - About 2.0x faster than ordinal `div` instruction on Intel Core i7-4790
- Dividing two `int` values quickly using `Int32Divisor`
  - About 1.4x faster than ordinal `idiv` instruction on Intel Core i7-4790

## Principles of operation

DivideSharp first computes the magic parameters for division and then converts a single division instruction, such as `idiv` or `div`, into an equivalent code that has no such slow division instructions and whose entire division code is faster than a single division instruction.

### The Mechanism for unsigned integer division

#### **The divisor is a power of two**

When the divisor is a power of two like 0b1000_0000u(=128u), the division can be simplified to a right-shift operation like:

```csharp
static uint D128(uint value) => value >> 7;
```

The above code is then compiled as follows(using [SharpLab](https://sharplab.io/)):

```asm
D128(UInt32)
    L0000: mov eax, ecx //The first parameter `value` is in `ecx` because of `static`.
    L0002: shr eax, 7   //Shift the eax 7 bits right
    L0005: ret          //return eax;
```

#### **The divisor is not a power of two**

The division of unsigned integers can theoretically be transformed into a set of multiplications and shifts, as shown in the following equation:  
![\lfloor \frac{value}{divisor} \rfloor = \lfloor \frac{value \times \frac{2^n}{divisor}}{2^n}\rfloor](https://latex.codecogs.com/svg.latex?%5Cdpi%7B300%7D%20%5Cfn_jvn%20%5Clfloor%20%5Cfrac%7Bvalue%7D%7Bdivisor%7D%20%5Crfloor%20%3D%20%5Clfloor%20%5Cfrac%7Bvalue%20%5Ctimes%20%5Cfrac%7B2%5En%7D%7Bdivisor%7D%7D%7B2%5En%7D%5Crfloor)  
For example, if the denominator is 239, we have the following formula:  
![magic = \lceil \frac{2^n}{239}\rceil = (2300233531)_{10} = (891AC73B)_{16}](https://latex.codecogs.com/svg.latex?%5Cdpi%7B300%7D%20%5Cfn_jvn%20magic%20%3D%20%5Clceil%20%5Cfrac%7B2%5En%7D%7B239%7D%5Crceil%20%3D%20%282300233531%29_%7B10%7D%20%3D%20%28891AC73B%29_%7B16%7D) (n=39 is chosen so that the magic is optimal for the required precision.)  
![\lfloor \frac{value}{239} \rfloor = \lfloor \frac{value \times 2300233531}{2^{32+7}}\rfloor](https://latex.codecogs.com/svg.latex?%5Cfn_jvn%20%5Clfloor%20%5Cfrac%7Bvalue%7D%7B239%7D%20%5Crfloor%20%3D%20%5Clfloor%20%5Cfrac%7Bvalue%20%5Ctimes%202300233531%7D%7B2%5E%7B32&plus;7%7D%7D%5Crfloor)  
Furthermore, since division by ![2^n](https://latex.codecogs.com/svg.latex?%5Cdpi%7B300%7D%20%5Cfn_jvn%20%5Csmall%202%5En) can be converted to a shift operation, this formula can be rewritten into the following C# code:  

```csharp
public static uint D239Custom(uint value)
{
    //In C#, the `mul` instruction used by real-world compilers such as RyuJIT and Clang cannot be specified directly, so the 64-bit unsigned multiplication `imul` is used instead.
    ulong v = value * 0x891ac73b;
    v >>= 39;
    return (uint)v;
}
```

The above code is then compiled as follows:

```asm
D239Custom(UInt32)
    L0000: imul eax, ecx, 0x891ac73b
    L0006: mov eax, eax
    L0008: shr rax, 0x27
    L000c: ret
```

#### **The divisor is not a power-of-two number** complex cases

However, codes like D239Custom may return inaccurate results depending on the denominator (e.g. 231).  
RyuJIT uses the following expression instead.  
![edx \leftarrow \lfloor \frac{value \times magic}{2^{32}}\rfloor](https://latex.codecogs.com/svg.latex?%5Cdpi%7B300%7D%20%5Cfn_jvn%20edx%20%5Cleftarrow%20%5Clfloor%20%5Cfrac%7Bvalue%20%5Ctimes%20magic%7D%7B2%5E%7B32%7D%7D%5Crfloor) (store the value of ![\lfloor \frac{value \times magic}{2^{32}}\rfloor](https://latex.codecogs.com/svg.latex?\fn_jvn&space;\tiny&space;\lfloor&space;\frac{value&space;\times&space;magic}{2^{32}}\rfloor) into `edx`)  
then calculate  
![\lfloor \frac{value}{divisor} \rfloor = \lfloor \frac{\lfloor edx + \lfloor\frac{value - edx}{2}\rfloor}{2^{n - 32}}\rfloor](https://latex.codecogs.com/svg.latex?%5Cfn_jvn%20%5Clfloor%20%5Cfrac%7Bvalue%7D%7Bdivisor%7D%20%5Crfloor%20%3D%20%5Clfloor%20%5Cfrac%7B%5Clfloor%20edx%20&plus;%20%5Clfloor%5Cfrac%7Bvalue%20-%20edx%7D%7B2%7D%5Crfloor%7D%7B2%5E%7Bn%20-%2032%7D%7D%5Crfloor)  
(The variable "edx" is named after the AMD64 32-bit register edx.)  
And the aforementioned equation can be organized as follows:  
![\begin{align*}
\lfloor\frac{value - \lfloor \frac{value \times magic}{2^{32}}\rfloor}{2}\rfloor &= \lfloor\frac{value - value \times \frac{magic}{2^{32}}}{2}\rfloor \\ 
 &= \lfloor\frac{value \times \frac{2^{32}}{2^{32}} - value \times \frac{magic}{2^{32}}}{2}\rfloor \\ 
 &= \lfloor\frac{value \times (\frac{2^{32} - magic}{2^{32}})}{2}\rfloor \\ 
 &= \lfloor value \times \frac{(\frac{2^{32} - magic}{2^{32}})}{2}\rfloor \\ 
 &= \lfloor value \times (\frac{2^{32} - magic}{2^{33}})\rfloor \\ 
\end{align*}]( https://latex.codecogs.com/svg.latex?%5Cdpi%7B100%7D%20%5Cfn_jvn%20%5Cbegin%7Balign*%7D%20%5Clfloor%5Cfrac%7Bvalue%20-%20%5Clfloor%20%5Cfrac%7Bvalue%20%5Ctimes%20magic%7D%7B2%5E%7B32%7D%7D%5Crfloor%7D%7B2%7D%5Crfloor%20%26%3D%20%5Clfloor%5Cfrac%7Bvalue%20-%20value%20%5Ctimes%20%5Cfrac%7Bmagic%7D%7B2%5E%7B32%7D%7D%7D%7B2%7D%5Crfloor%20%5C%5C%20%26%3D%20%5Clfloor%5Cfrac%7Bvalue%20%5Ctimes%20%5Cfrac%7B2%5E%7B32%7D%7D%7B2%5E%7B32%7D%7D%20-%20value%20%5Ctimes%20%5Cfrac%7Bmagic%7D%7B2%5E%7B32%7D%7D%7D%7B2%7D%5Crfloor%20%5C%5C%20%26%3D%20%5Clfloor%5Cfrac%7Bvalue%20%5Ctimes%20%28%5Cfrac%7B2%5E%7B32%7D%20-%20magic%7D%7B2%5E%7B32%7D%7D%29%7D%7B2%7D%5Crfloor%20%5C%5C%20%26%3D%20%5Clfloor%20value%20%5Ctimes%20%5Cfrac%7B%28%5Cfrac%7B2%5E%7B32%7D%20-%20magic%7D%7B2%5E%7B32%7D%7D%29%7D%7B2%7D%5Crfloor%20%5C%5C%20%26%3D%20%5Clfloor%20value%20%5Ctimes%20%28%5Cfrac%7B2%5E%7B32%7D%20-%20magic%7D%7B2%5E%7B33%7D%7D%29%5Crfloor%20%5C%5C%20%5Cend%7Balign*%7D )  
![\begin{align*}
\lfloor \frac{\lfloor \lfloor \frac{value \times magic}{2^{32}}\rfloor + \lfloor\frac{value - \lfloor \frac{value \times magic}{2^{32}}\rfloor}{2}\rfloor}{2^{n - 32}}\rfloor &= \lfloor \frac{ \frac{value \times magic}{2^{32}} + value \times \frac{2^{32} - magic}{2^{33}}}{2^{n - 32}}\rfloor \\ 
 &= \lfloor \frac{ value \times \frac{magic}{2^{32}} + value \times \frac{2^{32} - magic}{2^{33}}}{2^{n - 32}}\rfloor \\ 
 &= \lfloor \frac{value \times (\frac{2^{32} - magic + 2magic}{2^{33}})}{2^{n - 32}}\rfloor \\ 
 &= \lfloor \frac{value \times (\frac{2^{32} + magic}{2^{33}})}{2^{n - 32}}\rfloor \\ 
 &= \lfloor \frac{value \times (2^{32} + magic)}{2^{n + 1}}\rfloor \\ 
\end{align*}]( https://latex.codecogs.com/svg.latex?%5Cdpi%7B100%7D%20%5Cfn_jvn%20%5Cbegin%7Balign*%7D%20%5Clfloor%20%5Cfrac%7B%5Clfloor%20%5Clfloor%20%5Cfrac%7Bvalue%20%5Ctimes%20magic%7D%7B2%5E%7B32%7D%7D%5Crfloor%20&plus;%20%5Clfloor%5Cfrac%7Bvalue%20-%20%5Clfloor%20%5Cfrac%7Bvalue%20%5Ctimes%20magic%7D%7B2%5E%7B32%7D%7D%5Crfloor%7D%7B2%7D%5Crfloor%7D%7B2%5E%7Bn%20-%2032%7D%7D%5Crfloor%20%26%3D%20%5Clfloor%20%5Cfrac%7B%20%5Cfrac%7Bvalue%20%5Ctimes%20magic%7D%7B2%5E%7B32%7D%7D%20&plus;%20value%20%5Ctimes%20%5Cfrac%7B2%5E%7B32%7D%20-%20magic%7D%7B2%5E%7B33%7D%7D%7D%7B2%5E%7Bn%20-%2032%7D%7D%5Crfloor%20%5C%5C%20%26%3D%20%5Clfloor%20%5Cfrac%7B%20value%20%5Ctimes%20%5Cfrac%7Bmagic%7D%7B2%5E%7B32%7D%7D%20&plus;%20value%20%5Ctimes%20%5Cfrac%7B2%5E%7B32%7D%20-%20magic%7D%7B2%5E%7B33%7D%7D%7D%7B2%5E%7Bn%20-%2032%7D%7D%5Crfloor%20%5C%5C%20%26%3D%20%5Clfloor%20%5Cfrac%7Bvalue%20%5Ctimes%20%28%5Cfrac%7B2%5E%7B32%7D%20-%20magic%20&plus;%202magic%7D%7B2%5E%7B33%7D%7D%29%7D%7B2%5E%7Bn%20-%2032%7D%7D%5Crfloor%20%5C%5C%20%26%3D%20%5Clfloor%20%5Cfrac%7Bvalue%20%5Ctimes%20%28%5Cfrac%7B2%5E%7B32%7D%20&plus;%20magic%7D%7B2%5E%7B33%7D%7D%29%7D%7B2%5E%7Bn%20-%2032%7D%7D%5Crfloor%20%5C%5C%20%26%3D%20%5Clfloor%20%5Cfrac%7Bvalue%20%5Ctimes%20%282%5E%7B32%7D%20&plus;%20magic%29%7D%7B2%5E%7Bn%20&plus;%201%7D%7D%5Crfloor%20%5C%5C%20%5Cend%7Balign*%7D )  
Since ![2^{32}](https://latex.codecogs.com/svg.latex?%5Cinline%20%5Cfn_jvn%20%5Csmall%202%5E%7B32%7D) does not fit into the 32-bit unsigned integer variable `magic`, term ![2^{32} + magic](https://latex.codecogs.com/svg.latex?%5Cdpi%7B300%7D%20%5Cfn_jvn%20%5Csmall%202%5E%7B32%7D%20&plus;%20magic) adds ![2^{32}](https://latex.codecogs.com/svg.latex?%5Cinline%20%5Cfn_jvn%20%5Csmall%202%5E%7B32%7D) to the numerator stored in `magic`.  
So the `magic` is given by the formula below:  
![magic = \lceil \frac{2^n}{divisor}\rceil - 2^{32}](https://latex.codecogs.com/svg.latex?%5Cfn_jvn%20magic%20%3D%20%5Clceil%20%5Cfrac%7B2%5En%7D%7Bdivisor%7D%5Crceil%20-%202%5E%7B32%7D) (n is chosen so that ![\lceil \frac{2^n}{divisor}\rceil](https://latex.codecogs.com/svg.latex?%5Cinline%20%5Cfn_jvn%20%5Clceil%20%5Cfrac%7B2%5En%7D%7Bdivisor%7D%5Crceil) is the largest value less than ![2^{33}](https://latex.codecogs.com/svg.latex?%5Cinline%20%5Cfn_jvn%20%5Csmall%202%5E%7B33%7D))  

Now, when the divisor is 231, we can rewrite this expression in the following C# code:

```csharp
public static uint D231Custom(uint value)
{
    ulong v = value * 0x11bb4a405u;
    v >>= 40;
    return (uint)v;
}
```

The above code is then compiled as follows:

```asm
D231Custom(UInt32)
    L0000: mov eax, ecx
    L0002: mov rdx, 0x11bb4a405
    L000c: imul rax, rdx
    L0010: shr rax, 0x28
    L0014: ret
```

#### **Corner Cases**

If the denominator is greater than 2147483648(0x8000_0000u), as in 2147483649, the numerator cannot be more than twice its denominator.  
For this reason, it is more efficient to use the if statement instead of dividing by the method described above.
In C# it looks like the following:  

```csharp
//The Unsafe class belongs to System.Runtime.CompilerServices
public static uint D2147483649(uint value) => value >= 2147483649 ? 1u : 0u;
public static uint D2147483649Ex(uint value)    //Equivalent to value >= 2147483649 ? 1u : 0u
{
    bool f = value >= 2147483649;       //`cmp ecx, 0x80000001` and `setae al`
    return Unsafe.As<bool, byte>(ref f); //moxzx eax, al
}
```

The above code is then compiled as follows:

```asm
D2147483649(UInt32)     //Ternary operator
    L0000: cmp ecx, 0x80000001
    L0006: jae short L000b
    L0008: xor eax, eax
    L000a: ret
    L000b: mov eax, 1
    L0010: ret

D2147483649Ex(UInt32)   //Unsafe.As<bool, byte>
    L0000: cmp ecx, 0x80000001
    L0006: setae al
    L0009: movzx eax, al
    L000c: ret
```

#### **Runtime Optimizations**

The UInt32Divisor generalizes the four cases mentioned above with the following code:

```csharp
public uint Divide(uint value)
{
    uint strategy = (uint)Strategy;
    uint divisor = Divisor;
    if (strategy == (uint)UInt32DivisorStrategy.Branch)
    {
        bool v = value >= divisor;
        return Unsafe.As<bool, byte>(ref v);
    }
    ulong rax = value;
    uint eax;
    ulong multiplier = Multiplier;
    int shift = Shift;
    if ((strategy & 0b10u) > 0)
    {
        multiplier |= (strategy & 0b01ul) << 32;
        rax *= multiplier;
    }
    eax = (uint)(rax >> shift);
    return eax;
}
```

## Usage

### UInt32Divisor

<details>
  <summary>Details</summary>

#### Initialization

```csharp
var uInt32Divisor = new UInt32Divisor(19);
```

#### Methods

##### Division

```csharp
var quotient = uInt32Divisor.Divide(39); //2
```

##### Modulus

```csharp
var remainder = uInt32Divisor.Modulo(39); //1
```

##### DivRem

- Unlike `Math.DivRem`, the `out` parameter is `quotient`, not `remainder`.

```csharp
var remainder = uInt32Divisor.DivRem(39, out var quotient);
//remainder: 1
//quotient: 2
```

##### Floor

- Calculates the largest multiple of divisor less than or equal to the specified value.

```csharp
var rounded = uInt32Divisor.Floor(39); //38
```

##### FloorRem

```csharp
var remainder = uInt32Divisor.Floor(39, out var rounded);
//remainder: 1
//rounded: 38
```

#### Operator Overloads

##### Division

```csharp
var quotient = 38 / uInt32Divisor;  //2
```

##### Modulus(Modulo)

```csharp
var remainder = 39 % uInt32Divisor; //1
```

</details>

### Int32Divisor

<details>
  <summary>Details</summary>

#### Initialization

```csharp
var dn19 = new Int32Divisor(-19);   //Divisor -19
var dp19 = new Int32Divisor(19);    //Divisor 19
```

#### Methods

##### Division

```csharp
var quotient = dn19.Divide(39); //-2
var quotient2 = dn19.Divide(-39); //2
```

##### Modulus(Modulo)

```csharp
var remainder = dn19.Modulo(39); //1
var remainder2 = dn19.Modulo(-39); //-1
```

##### DivRem

- Unlike `Math.DivRem`, the `out` parameter is `quotient`, not `remainder`.

```csharp
var remainder = dn19.DivRem(39, out var quotient);
//remainder: 1
//quotient: -2
```

##### AbsFloor

- Divides the value with divisor, truncates the quotient **towards zero**, and multiplies the quotient with divisor.
  - Equivalent to (int)(value / divisor) _ divisor.
  - **NOT** Equivalent to `(int)Math.Floor(value / (double)divisor) _ divisor` which internally truncates the value towards negative infinity.

```csharp
var rounded = dn19.Floor(39); //It returns 38 unlike the (int)Math.Floor(39 / -19.0) * -19 returns 57
var rounded2 = dn19.Floor(-39); //-38
var rounded3 = dp19.Floor(-39); //It returns -38 unlike the (int)Math.Floor(-39 / 19.0) * 19 returns -57
var rounded4 = dp19.Floor(39); //38
```

##### AbsFloorRem

- Calculates `rounded = value / divisor * divisor, remainder = value - rounded`

```csharp
var remainder = dn19.Floor(39, out var rounded);
//remainder: 1
//rounded: 38
```

#### Operator Overloads

##### Division

```csharp
var quotient = 38 / dn19;  //-2
```

##### Modulus(Modulo)

```csharp
var remainder = 39 % dn19; //1
```

</details>
