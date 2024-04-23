import fastapi
import ga 
import argparse
import time


app = fastapi.FastAPI()

n_population = 20
population = ga.init_population(n_population)




@app.get("/populationSize")
async def populationSize():
    return n_population

@app.get("/getActions")
async def sendActions(  inputs: list[float]):
   return ga.jump(inputs.inputs, population)

@app.post("/sendFitnesses")
async def getFitnesses(fitnesses: list[float]):
    population = ga.evolution_step(population, fitnesses)

    