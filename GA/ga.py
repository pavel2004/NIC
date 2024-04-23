import numpy as np
import os 
n_population = 10
state_dim = 4
action_n = 1
layers = [state_dim] + [3] + [4] + [3] + [action_n]
level_length = 220

def relu(x):
  return np.where(x > 0, x, 0)

def sigmoid(x):
  return 1 / (1 + np.exp(-x))

def init_population(n_population):
  if os.path.exists('best.npy'):
    population = np.load('best.npy')
    population += np.random.randn(*population.shape) * 1e-2
  else:
    params = sum([(i + 1) * o for i, o in zip(layers[:-1], layers[1:])])
    population = np.random.randn(n_population, params)
  return population

def linear(x, weights):
  x_in = np.concatenate(([1], x))
  out = np.dot(weights, x_in)
  return out

def jump(state, population):
  out = []
  for j, p in enumerate(population):
    last_idx = 0
    x = np.array(state[j])
    for l, (i, o) in enumerate(zip(layers[:-1], layers[1:])):
      weights = p[last_idx:last_idx + (i + 1) * o].copy().reshape(o, (i + 1))
      last_idx += (i + 1) * o
      x = linear(x, weights)
      if l != len(layers) - 2:
        x = relu(x)
      else:
        x = sigmoid(x)
    out.append(x[0])
  print(np.array(out) > 0.5)
  return np.array(out) > 0.5

def sort_population_by_fitness(population_set, fitness_list, k):
    fitInd = np.argsort(fitness_list)
    sorted_population = population_set[fitInd[::-1]]
    sorted_fitness = fitness_list[fitInd[::-1]]
    return sorted_population[:k], sorted_fitness[:k]

def crossover(father_a, father_b):
    crossover_prob = 0.7
    child_layers = []
    last_idx = 0
    for l, (i, o) in enumerate(zip(layers[:-1], layers[1:])):
        weights_a = father_a[last_idx:last_idx + (i + 1) * o].copy().reshape(o, (i + 1))
        weights_b = father_b[last_idx:last_idx + (i + 1) * o].copy().reshape(o, (i + 1))
        child_layer = np.zeros_like(weights_a)
        last_idx += (i + 1) * o
        for j in range(weights_a.shape[0]):
            if np.random.random() < crossover_prob:
                child_layer[j] = weights_a[j]
            else:
                child_layer[j] = weights_b[j]
        child_layers.append(child_layer.copy())
    child = np.concatenate([c.flatten() for c in child_layers])
    
    return child

def mutate(individual, std, mutation_scale):
  mutate_prob = 0.8
  mutated = np.zeros_like(individual)
  for i in range(individual.shape[0]):
    if np.random.random() < mutate_prob:
      mutated[i] = individual[i] + np.random.random() * mutation_scale
    else:
      mutated[i] = individual[i]
  return mutated

def mutate_population(new_population, std, mutation_scales):
    mutated_pop = []
    for i in range(len(new_population)):
        mutated_pop.append(mutate(new_population[i], std, mutation_scales[i]))
    return np.array(mutated_pop)

def update_population(best_pop):

    new_population = best_pop.copy()
    while len(new_population) < n_population:
        father_a = best_pop[np.random.choice(np.arange(len(best_pop)))]
        father_b = best_pop[np.random.choice(np.arange(len(best_pop)))]

        # if (father_a == father_b).all():
        #   continue

        new_chromosome = crossover(father_a, father_b)
        new_population = np.vstack((new_population, new_chromosome))

    return new_population

# Initialize Population

# Initialize variables for the global best solution
best_global_fitness = float('inf')
cur_best = []
def evolution_step(population, fitness):
    global best_global_fitness
    global cur_best
    # Evaluate Fitness
    #fitness = get_all_fitness(population_set, cities_dict)
    # Check for the best solution in the current generation

    mutation_scales = 1 - fitness / level_length

    current_gen_best_fitness = np.max(fitness)
    current_gen_best_index = np.argmax(fitness)
    cur_best = population[current_gen_best_index]
    # Update the global best solution if it's better than the current best
    if current_gen_best_fitness > best_global_fitness:
        best_global_fitness = current_gen_best_fitness

    # Select the top individuals
    sorted_population, _ = sort_population_by_fitness(population, fitness, k=1)

    # Create new population through crossover
    new_population = update_population(sorted_population)
    std = new_population.std(axis=0)
    # Apply mutation
    mutated_population = mutate_population(new_population, std, mutation_scales)

    mutated_population = (mutated_population - mutated_population.mean()) / mutated_population.std()
    mutated_population[0] = cur_best
    np.save(file="best", arr=mutated_population)
    # Update the population set for the next generation
    return mutated_population



