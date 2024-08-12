import itertools
import threading
import time

filePath = "/Users/marcusbager/Desktop/Skole Privat/skole/Hf2/ObjProgrammering/5-letter-words/5-letter-words/Console/5-letter-words/perfect-words-big.txt"
# filePath = "/Users/marcusbager/Desktop/Skole Privat/skole/Hf2/ObjProgrammering/5-letter-words/5-letter-words/Console/5-letter-words/perfect-words-small.txt"
allWordsString = []
allWords = []
letterFrequency = {}

maxWords = 5
maxWordLength = 5
counter = itertools.count()


def readFile(path):
    with open(path) as file:
        return file.read().splitlines()


def getStringValue(word):
    value = 0
    for letter in word:
        if value == 0:
            value = letterFrequency[letter]
        elif letterFrequency[letter] < value:
            value = letterFrequency[letter]
    return value


def setLetterFrequency():
    for word in allWords:
        for letter in word:
            if letter in letterFrequency:
                letterFrequency[letter] += 1
            else:
                letterFrequency[letter] = 1


def sortAllWords():
    return list(filter(lambda x: len(x) == 5 and len(set(x)) == 5, allWords))


def getBitMask(word):
    mask = 0
    for letter in word:
        mask |= 1 << (ord(letter) - ord("a"))
    return mask


def setBitMask():
    allWords = []
    for word in allWordsString:
        mask = getBitMask(word)
        allWords.append(mask)
    return allWords


def find5Words(mask, iterator, currentCombination, depth):
    currentCombination.append(iterator)

    if depth == maxWords - 1:
        next(counter)
        combination = " ".join(
            [allWordsString[currentCombination[i]] for i in range(maxWords)]
        )
        print(combination)
        return

    for i in range(iterator + 1, len(allWords)):
        if (allWords[i] & mask) == 0:
            find5Words(mask | allWords[i], i, currentCombination, depth + 1)


start = time.perf_counter()
allWords = readFile(filePath)
allWords = sortAllWords()
setLetterFrequency()

allWords.sort(key=lambda x: getStringValue(x))
allWordsString = allWords
allWords = setBitMask()


firstWords = allWords[: letterFrequency["j"] + letterFrequency["q"]]


threads = []
for word in firstWords:
    thread = threading.Thread(
        target=find5Words, args=(word, allWords.index(word), [], 0)
    )
    thread.start()
    threads.append(thread)

    # find5Words(word, allWords.index(word), [], 0)

for thread in threads:
    thread.join()

end = time.perf_counter()
print(counter)
print(f"Finished in {round(end-start, 5)} second(s)")
print(f"{round(end-start, 5)*1000} millisecond(s)")
