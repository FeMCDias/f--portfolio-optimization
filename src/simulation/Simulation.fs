module Simulation

open System
open Metrics

/// Gera vetor de pesos (soma=1, cada ≤ maxW) por rejeição.
let private genWeights (n: int) (maxW: float) (rnd: Random) : float[] =
    let rec loop () =
      let raw = Array.init n (fun _ -> rnd.NextDouble())
      let sum = Array.sum raw
      let w = raw |> Array.map (fun x -> x / sum)
      if w |> Array.exists (fun x -> x > maxW) then loop() else w
    loop()

/// Simula nSim portfólios e retorna array de (pesos, Sharpe).
let simulate (returns: float[][]) (nSim: int) (maxW: float) : (float[] * float)[] =
    let rnd = Random()
    Array.init nSim (fun _ ->
      let w = genWeights returns.Length maxW rnd
      w, portfolioSharpe returns w)
