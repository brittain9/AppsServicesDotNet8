using System.Security.Cryptography;

namespace Packt.Shared;

public class Animal
{
[Coder("Mark Price", "22 August 2023")]
[Coder("Johnni Rasmussen", "13 September 2023")]
[Obsolete($"use {nameof(SpeakBetter)} instead.")]
public void Speak()
{
  WriteLine("Woof...");
}

public void SpeakBetter()
{
  WriteLine("Wooooooooof...");
}
}