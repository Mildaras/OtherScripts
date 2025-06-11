import numpy as np
import matplotlib.pyplot as plt

# -----------------------------------------------------------------------------
# 1) Konstantos ir funkcijos Δr ln(Kp) skaičiavimui:
# -----------------------------------------------------------------------------
R1 = -8.3          # [J/(mol·K)] skaitiklis pirmajam terminui
R2 =  0.01157      # [J/(mol·K^2)] skaitiklis antram terminui
R3 = 909000        # [J·K/mol] skaitiklis trečiam terminui
R4 = 115231        # [J·K/mol] skaitiklis ketvirtam terminui
Rg = 8.314         # [J/(mol·K)]

def ln_Kp(T):
    """
    Apskaičiuoja ln(Kp) pagal:
      ln(Kp) = (R1/Rg)*ln(T) + (R2/(2Rg))*T − (R3/(2Rg))*(1/T^2) − (R4/Rg)*(1/T) + 60.9
    """
    term1 = (R1 / Rg) * np.log(T)
    term2 = (R2 / (2 * Rg)) * T
    term3 = - (R3 / (2 * Rg)) * (1.0 / T**2)
    term4 = - (R4 / Rg) * (1.0 / T)
    const = 60.9
    return term1 + term2 + term3 + term4 + const

# -----------------------------------------------------------------------------
# 2) Funkcija f(α, Kp), kurios šaknį (f=0) randame bisection metodu:
# -----------------------------------------------------------------------------
def f_alpha(alpha, Kp):
    # Ieškosime α intervale [-0.9999, 0.9999]:
    if alpha <= -1.0 or alpha >= 1.0:
        return np.inf

    denom = 5.0 - alpha
    if denom <= 0:
        return np.inf

    num1 = (2.0 + 2.0 * alpha) / denom       # (2 + 2α)/(5 − α)
    num2 = (2.0 - 2.0 * alpha) / denom       # (2 − 2α)/(5 − α)
    num3 = (1.0 - alpha) / denom             # (1 − α)/(5 − α)

    if num3 < 0:
        return np.inf

    lhs = num1 / (num2 * np.sqrt(num3))
    return lhs - Kp

def find_alpha_bisection(Kp, x_low=-0.9999, x_high=0.9999, tol=1e-8, max_iter=100):
    """
    Bisection metodas: ieško α tokiam Kp, kad f_alpha(α, Kp)=0.
    """
    f_low  = f_alpha(x_low,  Kp)
    f_high = f_alpha(x_high, Kp)
    if np.isinf(f_low) or np.isinf(f_high) or (f_low * f_high > 0):
        return np.nan

    a, b = x_low, x_high
    fa, fb = f_low, f_high

    for _ in range(max_iter):
        c  = 0.5 * (a + b)
        fc = f_alpha(c, Kp)
        if np.abs(fc) < 1e-12 or (b - a) < tol:
            return c
        if fa * fc <= 0:
            b, fb = c, fc
        else:
            a, fa = c, fc

    return 0.5 * (a + b)

# -----------------------------------------------------------------------------
# 3) Funkcija ΔG apskaičiavimui iš ln(Kp):
# -----------------------------------------------------------------------------
R = 8.314  # J/(mol·K)

def deltaG_from_lnKp(T_arr, lnKp_arr):
    dG = np.zeros_like(T_arr, dtype=float)
    for i, (T, lnKp) in enumerate(zip(T_arr, lnKp_arr)):
        dG[i] = -R * T * lnKp
    return dG

# -----------------------------------------------------------------------------
# 4) Funkcija entalpijos apskaičiavimui:
# -----------------------------------------------------------------------------
dH298 = -114140      # Δ_rH°_298 [J/mol]
a     = -8.3         # Δ_r a [J/(mol·K)]
b     = 5.17e-3      # Δ_r b [J/(mol·K^2)]
d     = 9.09e5       # +Δ_r d, skaičiuojame d·(1/X − 1/298) [J·K/mol]

def entalpija_X(T):
    term1 = dH298
    term2 = a * (T - 298)
    term3 = (b / 2) * (T**2 - 298**2)
    term4 = d * (1.0 / T - 1.0 / 298.0)
    return term1 + term2 + term3 + term4

# -----------------------------------------------------------------------------
# 5) Temperatūrų sąrašas (T) ir apskaičiavimų seka:
# -----------------------------------------------------------------------------
T_list = np.array([230, 240, 250, 260, 270, 280], dtype=float)

# 1) Apskaičiuojame ln(Kp) kiekvienam T:
lnKp_list = ln_Kp(T_list)

# 2) Konvertuojame į Kp:
Kp_list = np.exp(lnKp_list)

# 3) Randame α kiekvienam Kp:
alpha_list = np.empty_like(Kp_list)
for i, Kp in enumerate(Kp_list):
    alpha_list[i] = find_alpha_bisection(Kp)

# 4) Apskaičiuojame xO2, xNO2 ir xNO pagal α:
#    xO2  = (1 − α)          / (5 − α)
#    xNO2 = (2 + 2 α)        / (5 − α)
#    xNO  = (2 − 2 α)        / (5 − α)
xO2_list   = (1.0 - alpha_list)       / (5.0 - alpha_list)
xNO2_list  = (2.0 + 2.0 * alpha_list)  / (5.0 - alpha_list)
xNO_list   = (2.0 - 2.0 * alpha_list)  / (5.0 - alpha_list)

# 5) Apskaičiuojame ΔG ir entalpiją kiekvienam T:
deltaG_list     = deltaG_from_lnKp(T_list, lnKp_list)
entalpijos_list = np.array([entalpija_X(T) for T in T_list], dtype=float)

# -----------------------------------------------------------------------------
# 6) Piešiame grafiką: ΔG vs T
# -----------------------------------------------------------------------------
plt.figure()
plt.plot(T_list, deltaG_list, marker='o')
plt.xlabel('T, K')
plt.ylabel('Δ_rG (J/mol)')
plt.title('Δ_rG priklausomybė nuo temperatūros')
plt.grid(True)
plt.axhline(0, color='red', linewidth=2)

# -----------------------------------------------------------------------------
# 7) Piešiame grafiką: α vs T
# -----------------------------------------------------------------------------
plt.figure()
plt.plot(T_list, alpha_list, marker='o')
plt.xlabel('T, K')
plt.ylabel('α')
plt.title('α priklausomybė nuo temperatūros')
plt.grid(True)
plt.axhline(0, color='black', linewidth=1)

# -----------------------------------------------------------------------------
# 8) Piešiame grafiką: xO2, xNO2 ir xNO viename grafike
# -----------------------------------------------------------------------------
plt.figure()
plt.plot(T_list, xO2_list, marker='o', label='O2')
plt.plot(T_list, xNO2_list, marker='o', label='NO2')
plt.plot(T_list, xNO_list, marker='o', label='NO')
plt.xlabel('T, K')
plt.ylabel('ci, %')
plt.title('O2, NO2 ir NO priklausomybės nuo temperatūros')
plt.legend()
plt.grid(True)

plt.show()
