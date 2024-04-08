from numpy.random import randint, rand

iterations = 200
bits = 300
pop_size = 1000
crossover_rate = 0.8
mutation_rate = 0.2


def fitness_for_pop(pop):
    fitness_pop = []
    for i in range(len(pop)):
        fitness_pop.append(fitness_indiv(pop[i]))
    return fitness_pop


def fitness_indiv(indiv):
    pass


def init_population(pop_len, indiv_len):
    population = []
    for i in range(pop_len):
        population.append(init_individual(indiv_len))
    return population


def init_individual(indiv_len):
    indiv = []
    for i in range(indiv_len):
        indiv.append(randint(0, 1))
    return indiv


# roulette wheel selection
def selection(pop, fitness):
    new_pop = []

    # cumulative probability
    sum_fit = sum(fitness)
    cum_prob = [fitness[0] / sum_fit]
    for i in range(1, len(fitness)):
        print(fitness[i] / sum_fit)
        cum_prob.append((fitness[i] / sum_fit) + cum_prob[i - 1])

    for i in range(len(pop)):
        prob = rand(0, 1)
        if cum_prob[0] >= prob:
            new_pop.append(pop[i])
        for j in range(1, len(cum_prob)):
            if cum_prob[j - 1] < prob <= cum_prob[j]:
                new_pop.append(pop[i])
    return new_pop


def crossover_for_pop(pop, cross_rate):
    for i in range(pop_size // 2):
        if rand(0, 1) < cross_rate:
            crossover(pop[i * 2], pop[i * 2 + 1])


def crossover(indiv1, indiv2):
    rand_range = randint(0, bits - 1)
    temp = indiv1[0:rand_range]
    indiv1[0:rand_range] = indiv2[0:rand_range]
    indiv2[0:rand_range] = temp


def mutation_for_pop(pop, mut_rate):
    for i in range(pop_size):
        if rand(0, 1) < mut_rate:
            crossover(pop[i * 2], pop[i * 2 + 1])


def mutation(indiv):
    rand_start = randint(0, bits - 2)
    rand_end = randint(rand_start, bits - 1)
    for i in range(rand_start, rand_end + 1):
        indiv[i] = abs(indiv[i] - 1)


def define_best(pop, fitness):
    max_ind = 0
    for i in range(len(fitness)):
        if fitness[i] < fitness[max_ind]:
            max_ind = i
    return pop[max_ind]


def genetic_algorithm(pop_len, indiv_len, cross_rate, mut_rate, num_iter):
    population = init_population(pop_len, indiv_len)
    iter_count = 0
    while iter_count < num_iter:
        iter_count += 1
        fitness = fitness_for_pop(population)
        population = selection(population, fitness)
        crossover_for_pop(population, cross_rate)
        mutation_for_pop(population, mut_rate)
    fitness = fitness_for_pop(population)
    return define_best(population, fitness)


genetic_algorithm(pop_size, bits, crossover_rate, mutation_rate, iterations)
