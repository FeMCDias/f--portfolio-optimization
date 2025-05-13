module Metrics

open System

/// Média de um array de floats.
let mean (xs: float[]) : float =
    Array.average xs

/// Desvio padrão amostral.
let std (xs: float[]) : float =
    let m = mean xs
    xs
    |> Array.averageBy (fun x -> (x - m) ** 2.0)
    |> sqrt

/// Matriz de covariância (N×N) de séries de retornos diários.
let covarianceMatrix (data: float[][]) : float[][] =
    if data.Length = 0 then [||]
    else
        let n = data.Length
        let days = data.[0].Length
        let means = data |> Array.map mean
        Array.init n (fun i ->
          Array.init n (fun j ->
            let si = data.[i]
            let sj = data.[j]
            let mi = means.[i]
            let mj = means.[j]
            Array.init days (fun d -> (si.[d] - mi) * (sj.[d] - mj))
            |> Array.average))

/// Retornos diários de uma carteira com pesos.
let portfolioDailyReturns (returns: float[][]) (weights: float[]) : float[] =
    let n = weights.Length
    let d = returns.[0].Length
    let result = Array.zeroCreate<float> d
    for i in 0..n-1 do
      let w = weights.[i]
      let series = returns.[i]
      for t in 0..d-1 do
        result.[t] <- result.[t] + series.[t] * w
    result

/// Retorno anualizado (mean * 252).
let portfolioAnnualReturn returns weights =
    portfolioDailyReturns returns weights
    |> mean
    |> fun m -> m * 252.0

/// Volatilidade anualizada (std * sqrt 252).
let portfolioAnnualVol returns weights =
    portfolioDailyReturns returns weights
    |> std
    |> fun s -> s * sqrt 252.0

/// Sharpe Ratio anualizado (sem risk-free). Zero se vol = 0.
let portfolioSharpe returns weights =
    let ar = portfolioAnnualReturn returns weights
    let av = portfolioAnnualVol returns weights
    if av = 0.0 then 0.0 else ar / av
