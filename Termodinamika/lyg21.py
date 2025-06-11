import numpy as np

# DUJŲ KONSTANTA:
R = 8.314  # J/(mol·K)

# Temperatūros (T) ir ln(Kp) sąrašai:
T_list   = np.array([230, 240, 250, 260, 270, 280], dtype=float)
lnKp_list = np.array([-5.6626, -3.1030, -0.7524, 1.4137, 3.4161, 5.2727], dtype=float)

# Patikrinimas, kad masyvai vienodo ilgio:
if T_list.shape != lnKp_list.shape:
    raise ValueError("T_list ir lnKp_list dydžiai nesutampa!")

# Funkcija, kuri apskaičiuoja ΔG tiesiog iš ln(Kp):
def deltaG_from_lnKp(T_arr, lnKp_arr):
    dG = np.zeros_like(T_arr, dtype=float)
    for i, (T, lnKp) in enumerate(zip(T_arr, lnKp_arr)):
        dG[i] = -R * T * lnKp
    return dG

# Apskaičiuojame ΔG kiekvienai temperatūrai:
deltaG_list = deltaG_from_lnKp(T_list, lnKp_list)

# Išvedame rezultatus lentelės formatu:
print("Temperatūra (K)   ln(Kp)      Δ_rG (J/mol)")
print("------------------------------------------")
for T, lnKp, dG in zip(T_list, lnKp_list, deltaG_list):
    print(f"   {T:7.1f}      {lnKp:8.4f}      {dG:12.2f}")

# Išvedame deltaG ir T sąrašus kaip Python list literal copy-paste’ui:
formatted_deltaG = "[" + ", ".join(f"{float(v):.4f}" for v in deltaG_list) + "]"
formatted_T      = "[" + ", ".join(f"{int(T)}" for T in T_list) + "]"

print("\nΔG list for copy-paste:")
print(formatted_deltaG)
print("T list for copy-paste:")
print(formatted_T)
